using System;
using Data_Management_for_Unity.Runtime.Databases;
using Data_Management_for_Unity.Runtime.Networking.Synchronising.Client;
using DMP.Utility;
using UnityEngine;

namespace Data_Management_for_Unity.Runtime.Objects
{
    public abstract class SynchronisedObject
    {
        public readonly string Id;
        
        [PreventSerialization]
        private Database _database;

        protected SynchronisedObject(bool isPersistent = true) : this(Guid.NewGuid().ToString(), isPersistent)
        {
            
        }

        protected SynchronisedObject(string id, bool isPersistent)
        {
            Id = id;
            _database = new Database(id, isPersistent, true);
        }

        public Database GetDatabase()
        {
            if (_database != null) return _database;

            if (SynchronisedClient.Instance == null)
                throw new InvalidOperationException("Synchronised object must be managed by a local SynchronisedClient");

            _database = SynchronisedClient.Instance.GetDatabase(Id, true);

            return _database;
        }

        public void Delete()
        {
            //start deletion process
            GetDatabase().Delete();
            
            //clear local reference
            _database = null;
        }
    }
}