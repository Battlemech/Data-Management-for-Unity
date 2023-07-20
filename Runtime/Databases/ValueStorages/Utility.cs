using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data_Management_for_Unity.Runtime.Serializer;

namespace Data_Management_for_Unity.Runtime.Databases.ValueStorages
{
    public static class Utility
    {
        public static Task Add<TCollection, TValue>(this ValueStorage<TCollection> valueStorage, TValue toAdd)
            where TCollection : ICollection<TValue>, new()
        {
            //serialize added value
            byte[] addedValue;
            Type addedType;

            //serialize entire collection
            byte[] collectionValue;
            Type collectionType;
            
            lock (valueStorage.Id)
            {
                //init collection if necessary
                valueStorage.Data ??= new TCollection();
                
                //add value to collection
                valueStorage.Data.Add(toAdd);
                
                //serialize added value
                addedValue = Serialization.Serialize(toAdd, out addedType);
                
                //serialize collection
                collectionValue = Serialization.Serialize(valueStorage.Data, out collectionType);
            }
            
            //process add internally
            return valueStorage.Database.OnAdd<TCollection, TValue>(valueStorage.Id, collectionValue, collectionType, addedValue, addedType);
        }

        public static Task Remove<TCollection, TValue>(this ValueStorage<TCollection> valueStorage, TValue toRemove)
            where TCollection : ICollection<TValue>, new()
        {
            //serialize added value
            byte[] removedValue;
            Type removedType;

            //serialize entire collection
            byte[] collectionValue;
            Type collectionType;
            
            lock (valueStorage.Id)
            {
                //init collection if necessary
                valueStorage.Data ??= new TCollection();
                
                //remove value from collection
                valueStorage.Data.Remove(toRemove);
                
                //serialize removed value
                removedValue = Serialization.Serialize(toRemove, out removedType);
                
                //serialize collection
                collectionValue = Serialization.Serialize(valueStorage.Data, out collectionType);
            }

            //process remove internally
            return valueStorage.Database.OnRemove<TCollection, TValue>(valueStorage.Id, collectionValue, collectionType, removedValue, removedType);
        }

        public static Task Add<TCollection, TKey, TValue>(this ValueStorage<TCollection> valueStorage, TKey key, TValue value)
            where TCollection : IDictionary<TKey, TValue>, new()
        {
            return valueStorage.Add(new KeyValuePair<TKey, TValue>(key, value));
        }

        public static Task RemoveKey<TCollection, TKey, TValue>(this ValueStorage<TCollection> valueStorage, TKey toRemove)
            where TCollection : IDictionary<TKey, TValue>, new()
        {
            //serialize added value
            byte[] removedValue;
            Type removedType;

            //serialize entire collection
            byte[] collectionValue;
            Type collectionType;
            
            lock (valueStorage.Id)
            {
                //init collection if necessary
                valueStorage.Data ??= new TCollection();
                
                //remove value from collection
                valueStorage.Data.Remove(toRemove);
                
                //serialize removed value
                removedValue = Serialization.Serialize(toRemove, out removedType);
                
                //serialize collection
                collectionValue = Serialization.Serialize(valueStorage.Data, out collectionType);
            }

            //process remove internally
            return valueStorage.Database.OnRemoveKey<TCollection, TKey, TValue>(valueStorage.Id, collectionValue, collectionType, removedValue, removedType);
        }
    }
}