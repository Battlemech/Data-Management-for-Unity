using System;
using System.Collections.Generic;
using Data_Management_for_Unity.Runtime.Databases.ValueStorages;
using Data_Management_for_Unity.Runtime.Persistence;
using Data_Management_for_Unity.Runtime.Serializer;

namespace Data_Management_for_Unity.Runtime.Databases
{
    public partial class Database
    {
        public bool IsPersistent
        {
            get => _isPersistent;
            set
            {
                //no change required
                if(value == _isPersistent) return;
                
                //invoke logic depending on new state
                if(value) OnPersistenceEnabled();
                else OnPersistenceDisabled();
                
                _isPersistent = value;
            }
        }

        private bool _isPersistent;

        /// <summary>
        /// Dict of values which could not be loaded to a value storage successfully
        /// </summary>
        private readonly Dictionary<string, SerializedObject> _toLoad = new Dictionary<string, SerializedObject>();

        private void OnPersistenceEnabled()
        {
            //create database in SQLite if it doesn't exist
            if (!PersistentData.TryLoadDatabase(Id, out List<SerializedObject> savedObjects))
            {
                //database didn't exist in SQL. Create it
                PersistentData.CreateDatabase(Id);
                return;
            }
            
            //lock database to avoid changes by other threads, e.g. synchronisation
            lock (_values)
            {
                //load values
                foreach (var savedObject in savedObjects)
                {
                    //persistently saved data isn't newer than already known one
                    if(!UpdateModCount(savedObject.ValueId, savedObject.ModCount)) continue;
                    
                    //if value storage exists
                    if (_values.TryGetValue(savedObject.ValueId, out ValueStorage valueStorage))
                    {
                        //update value
                        valueStorage.UnsafeSet(Serialization.Deserialize(savedObject.Value, savedObject.Type));
                        
                        //update mod count
                        continue;
                    }  

                    //save loaded object to be deserialized later
                    lock (_toLoad)
                    {
                        _toLoad.Add(savedObject.ValueId, savedObject);   
                    }
                }
       
            } 
        }

        private void OnPersistenceDisabled()
        {
            //delete old data
            PersistentData.DeleteDatabase(Id);
        }
    }
}