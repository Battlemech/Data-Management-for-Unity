using System.Collections.Generic;
using Data_Management_for_Unity.Runtime.Databases;
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

        protected override void OnConstructorCalled()
        {
            //update default local instance, if necessary
            if (Instance == null) Instance = this;
            
            //update values received by remote
            AddCallback<SetValueMessage>((message =>
            {
                GetDatabase(message.DatabaseId)
                    .OnRemoteSet(message.ValueId, message.Value, message.Type, message.ModCount);
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
                _databases.Add(database.Id, database);
            }
        }

        protected internal void RemoveDatabase(Database database)
        {
            lock (_databases)
            {
                _databases.Remove(database.Id);
            }
        }
        
        private Database GetDatabase(string id)
        {
            lock (_databases)
            {
                //return existing database
                if (_databases.TryGetValue(id, out Database database)) return database;
                
                //create new database referenced by remote
                //(it will automatically be added to local list of databases)
                database = new Database(id, isPersistent:PersistentData.DoesDatabaseExists(id), isSynchronised:true);

                //return new database
                return database;
            }
        }
    }
}