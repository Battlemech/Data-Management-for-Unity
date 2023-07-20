using System.Collections.Generic;
using System.Threading.Tasks;

namespace Data_Management_for_Unity.Runtime.Databases.ValueStorages
{
    public class DictStorage<TCollection, TKey, TValue> : ValueStorage<TCollection>
        where TCollection : IDictionary<TKey, TValue>, new()
    {
        public DictStorage(string id, Database database) : base(id, database)
        {
            
        }

        public Task Add(TKey key, TValue value) => Utility.Add(this, key, value);

        public Task Remove(TKey key) => this.RemoveKey<TCollection, TKey, TValue>(key);
    }
}