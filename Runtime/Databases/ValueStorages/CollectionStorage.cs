using System.Collections.Generic;
using System.Threading.Tasks;

namespace Data_Management_for_Unity.Runtime.Databases.ValueStorages
{
    public class CollectionStorage<TCollection, TValue> : ValueStorage<TCollection>
        where TCollection : ICollection<TValue>, new()
    {
        public CollectionStorage(string id, Database database) : base(id, database)
        {
            
        }

        public Task Add(TValue value) => Utility.Add(this, value);

        public Task Remove(TValue value) => Utility.Remove(this, value);
    }
}