using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data_Management_for_Unity.Runtime.Databases.Structs;
using Data_Management_for_Unity.Runtime.Databases.SynchronisedOperations;
using Data_Management_for_Unity.Runtime.Databases.ValueStorages;
using Data_Management_for_Unity.Runtime.Networking.Synchronising.Client;
using Data_Management_for_Unity.Runtime.Networking.Synchronising.Messages;
using Data_Management_for_Unity.Runtime.Objects;
using Data_Management_for_Unity.Runtime.Persistence3;
using Data_Management_for_Unity.Runtime.Serializer;
using Data_Management_for_Unity.Runtime.Threading;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Data_Management_for_Unity.Runtime.Databases
{
    public partial class Database
    {
        /// <summary>
        /// Client managing the synchronisation of the database. Per default, only one client exists locally.
        /// However, multiple may be used for testing.
        /// </summary>
        public SynchronisedClient Client;

        public bool IsSynchronised
        {
            get => _isSynchronised;
            set
            {
                //no update is necessary
                if(value == _isSynchronised) return;
                
                //invoke logic depending on new state
                if (value) OnSynchronisationEnabled();
                else OnSynchronisationDisabled();

                //update local value
                _isSynchronised = value;
            }
        }
        private bool _isSynchronised;

        private void OnSynchronisationEnabled()
        {
            //set a reference to synchronised client, if necessary
            if (Client == null) Client = SynchronisedClient.Instance;

            //At least one client must be available
            if (Client == null)
                throw new Exception("Database must be managed by a local SynchronisedClient to be synchronised!");
            
            //add database to list of local databases
            Client.AddDatabase(this);
            
            //share current value, but not children, in network to avoid cascading data sharing between games
            ShareInNetwork(false);
        }
        
        private void OnSynchronisationDisabled()
        {
            //remove database from list of local databases
            Client.RemoveDatabase(this);
        }

        protected internal void OnRemoteOperation(SynchronisedOperation operation)
        {
            //init default values with 0
            byte[] value = null;
            Type type = null;
            
            //try retrieving current values
            lock (_confirmed)
            {
                if (_confirmed.TryGetValue(operation.ValueId, out ValueRecord record))
                {
                    //data is already known. No need to process operation
                    if (record.ModCount >= operation.ModCount) return;
                    
                    value = record.Value;
                    type = record.Type;
                }
            }

            //process remote operation
            OnOperation(value, type, operation, false);
        }

        private void OnOperation(byte[] value, Type type, SynchronisedOperation operation, bool local)
        {
            while (true)
            {
                //extract basic information about target
                string id = operation.ValueId;
                int modCount = operation.ModCount;

                //repeat operation with up to date data
                value = local ? operation.Repeat(value, type, out type) : operation.OnRemote(value, type, out type);

                //update value locally
                lock (_values)
                {
                    //update value locally, if it exists
                    if (_values.TryGetValue(id, out ValueStorage storage))
                        storage.InternalSet(value, type);
                    //value can be loaded later
                    else
                        _toLoad[id] = new PersistentObject(Id, id, value, type, modCount);
                }

                //invoke callbacks. Deserializing the object again makes sure it isn't changed after update in ValueStorage
                Invoke(id, SerializationPCK.Deserialize(value, type));

                //update confirmed data
                lock (_confirmed) _confirmed[id] = new ValueRecord(value, type, modCount);
                
                if (local)
                {
                    //notify peers of new value
                    Client.Send(new OperationMessage(operation));
                    
                    //local operations may have a callback waiting to be executed on confirmation
                    operation.OnConfirmed(value, type);
                    
                    //locally executed safe operations need to persistently save data after remote confirmation
                    if(operation.IsSafeOperation()) PersistentData.Save(Id, id, value, type, modCount) ;
                }
                //save remote operations persistently, if necessary
                else if (IsPersistent)
                {
                    PersistentData.Save(Id, id, value, type, modCount);
                }

                //stop executing operations if no more delayed exist
                if (!TryDequeueDelayedOperation(id, modCount + 1, out operation)) return;

                //any remaining operations source will be local
                local = true;
            }
        }
    }
}