using System.Collections.Generic;
using System.Threading.Tasks;

namespace Data_Management_for_Unity.Runtime.Databases.ValueStorages
{
    public static class Utility
    {
        public static Task Add<TCollection, TData>(this ValueStorage<TCollection> valueStorage, TData toAdd)
            where TCollection : ICollection<TData>, new()
        {
            return valueStorage.Modify((collection =>
            {
                //init collection if necessary
                collection ??= new TCollection();
                
                //add value
                collection.Add(toAdd);

                //return updated collection
                return collection;
            }));
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