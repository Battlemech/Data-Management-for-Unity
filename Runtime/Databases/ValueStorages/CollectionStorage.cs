using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data_Management_for_Unity.Runtime.Serializer;

namespace Data_Management_for_Unity.Runtime.Databases.ValueStorages
{
    public class CollectionStorage<T, TValue> : ValueStorage<T> where T : ICollection<TValue>, new()
    {
        public CollectionStorage(string id, Database database) : base(id, database)
        {
            
        }

        public Task Add(TValue toAdd)
        {
            byte[] value;
            Type type;

            lock (Id)
            {
                //add value to collection
                Data.Add(toAdd);
                
                //serialize added value, not entire collection to improve performance
                value = Serialization.Serialize(toAdd, out type);
            }

            return Database.OnAdd(Id, value, type);
        }
    }
}