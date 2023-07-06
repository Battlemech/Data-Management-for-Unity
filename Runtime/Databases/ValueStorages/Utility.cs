using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data_Management_for_Unity.Runtime.Serializer;

namespace Data_Management_for_Unity.Runtime.Databases.ValueStorages
{
    public static class Utility
    {
        public static Task Add<TCollection, TData>(this ValueStorage<TCollection> vs, TData toAdd)
            where TCollection : ICollection<TData>, new()
        {
            //serialize value which is supposed to be added
            byte[] addedValue = Serialization.Serialize(toAdd, out Type addedType);
            
            //prepare serialization of collection
            byte[] collectionValue;
            Type collectionType;

            //make sure no other process modifies data
            lock (vs.Id)
            {
                //init collection if necessary
                vs.Data ??= new TCollection();
                
                //add value to collection
                vs.Data.Add(toAdd);
                
                //serialize current state of collection
                collectionValue = Serialization.Serialize(vs.Data, out collectionType);
            }

            //process add
            return vs.Database.OnAdd(vs.Id, collectionValue, collectionType, addedValue, addedType);
        }
    }
}