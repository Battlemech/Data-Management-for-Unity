using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data_Management_for_Unity.Runtime.Databases.ValueStorages;
using Data_Management_for_Unity.Runtime.Networking.Synchronising.Client;
using Data_Management_for_Unity.Runtime.Persistence;
using Data_Management_for_Unity.Runtime.Serializer;
using UnityEngine;

namespace Data_Management_for_Unity.Runtime.Databases
{
    public partial class Database
    {
        public bool IsPersistent { get; private set; }

        /// <summary>
        /// Dict of values which could not be loaded to a value storage successfully
        /// </summary>
        private readonly Dictionary<string, PersistentObject> _toLoad = new Dictionary<string, PersistentObject>();

        public async Task ConfigurePersistence(bool active)
        {
            //no changes required
            if(IsPersistent == active) return;
            
            if(active) await OnPersistenceEnabled();
            else OnPersistenceDisabled();
            
            //update flag
            IsPersistent = active;
        }
        
        private async Task OnPersistenceEnabled()
        {
            bool dataExists = await PersistentData2.DoesDatabaseExists(Id);

            //create database in SQLite if it doesn't exist
            if (!dataExists)
            {
                await PersistentData2.CreateDatabase(Id);
                return;
            }
            
            //load data from SQLite
            List<PersistentObject> savedObjects = await PersistentData2.Load(Id);
            
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
                }
       
            } 
        }

        private void OnPersistenceDisabled()
        {
            //delete old data
            PersistentData2.DeleteDatabase(Id);
        }
    }
}