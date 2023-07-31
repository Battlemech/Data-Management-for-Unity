using System;
using System.Collections.Generic;
using Data_Management_for_Unity.Runtime.Databases;
using Data_Management_for_Unity.Runtime.Databases.SynchronisedOperations;
using Data_Management_for_Unity.Runtime.Networking.Messaging.Client;
using Data_Management_for_Unity.Runtime.Networking.Synchronising.Messages;
using Data_Management_for_Unity.Runtime.Persistence;

namespace Data_Management_for_Unity.Runtime.Networking.Synchronising.Client
{
    public class SynchronisedClient : MessageClient
    {
        /// <summary>
        /// First created local instance of a synchronised client.
        /// Reference is removed once client is destroyed.
        /// </summary>
        public static SynchronisedClient Instance { get; private set; }
        
        /// <summary>
        /// Contains a list of all synchronised databases known to a local client
        /// </summary>
        private readonly Dictionary<string, Database> _databases = new Dictionary<string, Database>();

        protected override void Awake()
        {
            base.Awake();

            //update default local instance, if necessary
            if (Instance == null) Instance = this;
            
            //update values received by remote
            AddCallback<OperationMessage>((message =>
            {
                //extract operation for easier access
                SynchronisedOperation operation = message.GetOperation();
                
                //process operation
                GetDatabase(operation.DatabaseId).OnRemoteOperation(operation);
            }));
        }

        protected override void OnDestroy()
        {
            //disconnect, if connection is still active
            base.OnDestroy();
            
            //remove instance reference
            if (Instance == this) Instance = null;
        }

        protected internal void AddDatabase(Database database)
        {
            lock (_databases)
            {
                if(_databases.TryAdd(database.Id, database)) return;

                throw new InvalidOperationException($"Another synchronised database with id={database.Id} already exists! Use SynchronisedClient.GetDatabase() to make retrieve existing databases!");
            }
        }

        protected internal void RemoveDatabase(Database database)
        {
            lock (_databases)
            {
                _databases.Remove(database.Id);
            }
        }
        
        public Database GetDatabase(string id, bool isSynchronised=true)
        {
            lock (_databases)
            {
                //return existing database
                if (_databases.TryGetValue(id, out Database database)) return database;
                
                //create new database referenced by remote
                //(it will automatically be added to local list of databases)
                database = new Database(id, isPersistent:PersistentData.DoesDatabaseExists(id), isSynchronised:isSynchronised);

                //return new database
                return database;
            }
        }
    }
}