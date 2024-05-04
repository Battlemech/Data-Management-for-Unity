using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data_Management_for_Unity.Runtime.Databases.ValueStorages;
using Data_Management_for_Unity.Runtime.Networking.Synchronising.Client;
using Data_Management_for_Unity.Runtime.Persistence;
using DMP.Threading;

namespace Data_Management_for_Unity.Runtime.Databases
{
    public partial class Database
    {
        public readonly string Id;

        /// <summary>
        /// Values stored in database
        /// </summary>
        private readonly Dictionary<string, ValueStorage> _values = new Dictionary<string, ValueStorage>();

        public Database(string id, bool isPersistent=false, bool isSynchronised=false, SynchronisedClient client=null)
        {
            Id = id;
            
            //synchronously configure persistence
            Task<bool> persistenceTask = Task.Run((() => ConfigurePersistence(isPersistent).Wait(Options.DefaultTimeout)));
            
            //wait for persistence to be configured
            bool success = persistenceTask.Wait(Options.DefaultTimeout);
            if (!success) throw new TimeoutException("Failed to configure persistence");
            
            
            //assign client (if any) before configuring synchronisation
            Client = client;
            IsSynchronised = isSynchronised;
        }

        public ValueStorage<T> Get<T>(string id)
        {
            lock (_values)
            {
                //if value exists locally
                if (_values.TryGetValue(id, out ValueStorage localStorage))
                {
                    //and is of the correct type: Return it
                    if (localStorage is ValueStorage<T> typedStorage) return typedStorage;

                    throw new ArgumentException($"Expected ValueStorage<{typeof(T)}>, but found {localStorage.GetType()}");
                }
            
                //create value
                ValueStorage<T> valueStorage = new ValueStorage<T>(id, this);
            
                //try loading saved value, if any
                lock (_toLoad)
                {
                    //if value exists
                    if (_toLoad.TryGetValue(id, out PersistentObject toLoad))
                    {
                        //try updating the new valueStorage to current value
                        valueStorage.InternalSet(toLoad.Value, toLoad.Type);
                    }
                }
                
                //add new value storage
                _values.Add(id, valueStorage);

                //return new created value storage
                return valueStorage;
            }
            
        }
    }
}