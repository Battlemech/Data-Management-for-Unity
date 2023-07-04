using System;
using System.Linq;
using System.Threading.Tasks;
using Data_Management_for_Unity.Runtime.Databases.DelayedOperations;
using Data_Management_for_Unity.Runtime.Databases.Structs;
using Data_Management_for_Unity.Runtime.Databases.ValueStorages;
using Data_Management_for_Unity.Runtime.Networking.Synchronising.Client;
using Data_Management_for_Unity.Runtime.Networking.Synchronising.Messages;
using Data_Management_for_Unity.Runtime.Persistence;
using Data_Management_for_Unity.Runtime.Serializer;
using UnityEngine;

namespace Data_Management_for_Unity.Runtime.Databases
{
    public partial class Database
    {
        /// <summary>
        /// Client managing the synchronisation of the client. Per default, only one client exists locally.
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
            if (Client.Equals(null)) Client = SynchronisedClient.Instance;
            
            //add database to list of local databases
            Client.AddDatabase(this);

            //synchronise values
            lock (_values)
            {
                foreach (var storage in _values.Values)
                {
                    //start informing peers about local values
                    OnSetSynchronised(storage.Id, storage.Serialize(out Type type), type, GetModCount(storage.Id))
                        //and make sure the task doesn't terminate with an exception
                        .ContinueWith((task =>
                        {
                            if(task.Exception != null) Debug.LogException(task.Exception);                            
                        }));
                }
            }
        }

        private void OnSynchronisationDisabled()
        {
            //remove database from list of local databases
            Client.RemoveDatabase(this);
        }

        /// <summary>
        /// Called when a remote client sets a value of this database
        /// </summary>
        protected internal void OnRemoteSet(string id, byte[] bytes, Type type, int modCount)
        {
            while (true)
            {
                //value is already known to database
                if (!UpdateConfirmedData(id, modCount, bytes, type)) return;

                lock (_values)
                {
                    //update value locally, if it exists
                    if (_values.TryGetValue(id, out ValueStorage storage))
                        storage.InternalSet(bytes, type);
                    //value can be loaded later
                    else
                        _toLoad.Add(id, new SerializedObject(Id, id, bytes, type, modCount));
                }

                //invoke callbacks. Deserializing the object again makes sure it isn't changed after update in ValueStorage
                _callbackHandler.Invoke(id, Serialization.Deserialize(bytes, type));

                //no delayed requests exist which need to be processed
                if (!TryDequeueDelayedOperation(id, modCount + 1, out DelayedOperation operation)) return;

                //repeat operation with up to date data
                bytes = operation.Invoke(bytes, type, out type);

                //notify peers of new value
                Client.Send(new SetValueMessage(Id, id, bytes, type, operation.ModCount));

                //process delayed set locally
                modCount = operation.ModCount;
            }
        }
    }
}