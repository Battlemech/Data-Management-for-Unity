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

        public static Task Remove<TCollection, TData>(this ValueStorage<TCollection> valueStorage, TData toRemove)
            where TCollection : ICollection<TData>, new()
        {
            return valueStorage.Modify((collection =>
            {
                //no value to remove
                if (collection == null) return default;
                
                //remove value
                collection.Remove(toRemove);

                //return updated collection
                return collection;
            }));
        }
    }
}