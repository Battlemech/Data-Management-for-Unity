using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data_Management_for_Unity.Runtime.Serializer;

namespace Data_Management_for_Unity.Runtime.Databases.ValueStorages
{
    public static class Utility
    {
        public static Task Add<TCollection, TValue>(this ValueStorage<TCollection> valueStorage, TValue toAdd, bool safe=false, Action<TCollection> onConfirmed=null)
            where TCollection : ICollection<TValue>, new()
        {
            //serialize added value
            byte[] addedValue = Serialization.Serialize(toAdd, out Type addedType);

            //serialize entire collection
            byte[] collectionValue;
            Type collectionType;
            
            lock (valueStorage.Id)
            {
                //init collection if necessary
                valueStorage.Data ??= new TCollection();
                
                //add value to collection
                valueStorage.Data.Add(toAdd);

                //serialize collection
                collectionValue = Serialization.Serialize(valueStorage.Data, out collectionType);
            }
            
            //process add internally
            return valueStorage.Database.OnAdd<TCollection, TValue>(valueStorage.Id, collectionValue, collectionType, addedValue, addedType, safe, onConfirmed);
        }

        public static Task Remove<TCollection, TValue>(this ValueStorage<TCollection> valueStorage, TValue toRemove, bool safe=false, Action<TCollection> onConfirmed=null)
            where TCollection : ICollection<TValue>, new()
        {
            //serialize removed value
            byte[] removedValue = Serialization.Serialize(toRemove, out Type removedType);

            //serialize entire collection
            byte[] collectionValue;
            Type collectionType;
            
            lock (valueStorage.Id)
            {
                //init collection if necessary
                valueStorage.Data ??= new TCollection();
                
                //remove value from collection
                valueStorage.Data.Remove(toRemove);

                //serialize collection
                collectionValue = Serialization.Serialize(valueStorage.Data, out collectionType);
            }

            //process remove internally
            return valueStorage.Database.OnRemove<TCollection, TValue>(valueStorage.Id, collectionValue, collectionType, removedValue, removedType, safe, onConfirmed);
        }

        public static Task Add<TCollection, TKey, TValue>(this ValueStorage<TCollection> valueStorage, TKey key, TValue value, bool safe=false, Action<TCollection> onConfirmed=null)
            where TCollection : IDictionary<TKey, TValue>, new()
        {
            return valueStorage.Add(new KeyValuePair<TKey, TValue>(key, value), safe, onConfirmed);
        }

        public static Task RemoveKey<TCollection, TKey, TValue>(this ValueStorage<TCollection> valueStorage, TKey toRemove, bool safe=false, Action<TCollection> onConfirmed=null)
            where TCollection : IDictionary<TKey, TValue>, new()
        {
            //serialize removed key
            byte[] removedValue = Serialization.Serialize(toRemove, out Type removedType);

            //serialize entire collection
            byte[] collectionValue;
            Type collectionType;
            
            lock (valueStorage.Id)
            {
                //init collection if necessary
                valueStorage.Data ??= new TCollection();
                
                //remove value from collection
                valueStorage.Data.Remove(toRemove);

                //serialize collection
                collectionValue = Serialization.Serialize(valueStorage.Data, out collectionType);
            }

            //process remove internally
            return valueStorage.Database.OnRemoveKey<TCollection, TKey, TValue>(valueStorage.Id, collectionValue, collectionType, removedValue, removedType, safe, onConfirmed);
        }

        public static bool TryGetValue<TCollection, TKey, TValue>(this ValueStorage<TCollection> valueStorage, TKey key, out TValue value)
            where TCollection : IDictionary<TKey, TValue>
        {
            //allow value to be modified in lambda
            TValue result = default;
            
            //try getting value
            bool success = valueStorage.BlockingGet((collection => collection != null && collection.TryGetValue(key, out result)));

            //save result in out parameter
            value = result;

            return success;
        }
        
        public static Task Update<TCollection, TKey, TValue>(this ValueStorage<TCollection> valueStorage, TKey key, TValue value, bool safe=false, Action<TCollection> onConfirmed=null)
            where TCollection : IDictionary<TKey, TValue>, new()
        {
            throw new NotImplementedException();
        }

        public static bool Contains<TCollection, TValue>(this ValueStorage<TCollection> valueStorage, TValue value)
            where TCollection : ICollection<TValue>
        {
            return valueStorage.BlockingGet((values => values.Contains(value)));
        }
    }
}