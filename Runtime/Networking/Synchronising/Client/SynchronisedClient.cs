using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data_Management_for_Unity.Runtime.Databases;
using Data_Management_for_Unity.Runtime.Databases.SynchronisedOperations;
using Data_Management_for_Unity.Runtime.Networking.Messaging.Client;
using Data_Management_for_Unity.Runtime.Networking.Synchronising.Messages;
using Data_Management_for_Unity.Runtime.Objects;
using Data_Management_for_Unity.Runtime.Persistence;
using Data_Management_for_Unity.Runtime.Serializer;

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
        /// Per default, only one instance of the database manager exists.
        /// However, for testing, multiple instances simulating multiple remote locations can be created
        /// </summary>
        public DatabaseManager DBManager = DatabaseManager.Instance;
        
        protected override void Awake()
        {
            base.Awake();

            //update default local instance, if necessary
            if (Instance == null) Instance = this;
            
            //update values received by remote
            AddCallback<OperationMessage>((message =>
            {
                //extract operation for easier access
                SynchronisedOperation operation = message.Operation;
                
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

        public Database GetDatabase(string id, bool isSynchronised = true, bool? isPersistent = null)
        {
            return DBManager.GetDatabase(id, isSynchronised, isPersistent);
        }
        
    }
}