using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data_Management_for_Unity.Runtime.Databases.ValueStorages;
using Data_Management_for_Unity.Runtime.Networking.Synchronising.Client;
using Data_Management_for_Unity.Runtime.Persistence3;
using Data_Management_for_Unity.Runtime.Serializer;
using UnityEngine;

namespace Data_Management_for_Unity.Runtime.Databases
{
    public partial class Database
    {
        public bool IsPersistent
        {
            get => _isPersistent;
            set
            {
                //no changes required
                if(IsPersistent == value) return;
            
                if(value) OnPersistenceEnabled();
                else OnPersistenceDisabled();
            
                //update flag
                _isPersistent = value;
            }
        }

        private bool _isPersistent;

        /// <summary>
        /// Dict of values which could not be loaded to a value storage successfully
        /// </summary>
        private readonly Dictionary<string, PersistentObject> _toLoad = new Dictionary<string, PersistentObject>();

        /// <summary>
        /// Tries loading current persistently saved value for a single valueStorage
        /// </summary>
        /// <param name="valueId"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryPersistentLoad(string valueId, out object value)
        {
            if (PersistentData.TryLoad(Id, valueId, out PersistentObject po))
            {
                value = po.Deserialize();
                return true;
            }
            
            value = null;
            return false;
        }
        
        private void OnPersistenceEnabled()
        {
            bool dataExists = PersistentData.DoesDatabaseExists(Id);

            //create database in SQLite if it doesn't exist
            if (!dataExists)
            {
                PersistentData.CreateDatabase(Id);
                return;
            }
            
            //load data from SQLite
            List<PersistentObject> savedObjects = PersistentData.Load(Id);
            
            //lock database to avoid changes by other threads, e.g. synchronisation
            lock (_values)
            {
                //load values
                foreach (var savedObject in savedObjects)
                {
                    //persistently saved data isn't newer than already known one
                    if(!UpdateConfirmedData(savedObject.ValueId, savedObject.ModCount, savedObject.Value, savedObject.Type)) continue;
                    
                    //if value storage exists
                    if (_values.TryGetValue(savedObject.ValueId, out ValueStorage valueStorage))
                    {
                        //update value
                        valueStorage.InternalSet(savedObject.Value, savedObject.Type);
                        continue;
                    }  

                    //save loaded object to be deserialized later
                    lock (_toLoad)
                    {
                        _toLoad.Add(savedObject.ValueId, savedObject);   
                    }
                    
                    //share current value, but not children, in network to avoid cascading data sharing between games
                    if(IsSynchronised) ShareInNetwork(false);
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