using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Data_Management_for_Unity.Runtime.Databases.ValueStorages;
using Data_Management_for_Unity.Runtime.Persistence;

namespace Data_Management_for_Unity.Runtime.Databases
{
    public partial class Database
    {
        public readonly string Id;

        private readonly Dictionary<string, ValueStorage> _values =
            new Dictionary<string, ValueStorage>();

        public Database(string id, bool isPersistent=false, bool isSynchronised=false)
        {
            Id = id;
            IsPersistent = isPersistent;
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
                    if (_toLoad.TryGetValue(id, out SerializedObject toLoad))
                    {
                        //try updating the new valueStorage to current value
                        valueStorage.InternalSet(toLoad.DeserializeObject());
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