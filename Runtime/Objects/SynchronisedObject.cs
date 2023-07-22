using System;
using Data_Management_for_Unity.Runtime.Databases;
using Data_Management_for_Unity.Runtime.Networking.Synchronising.Client;

namespace Data_Management_for_Unity.Runtime.Objects
{
    public class SynchronisedObject
    {
        public readonly string Id;
        
        [NonSerialized]
        private Database _database;

        public SynchronisedObject(string id, bool isPersistent)
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
    }
}