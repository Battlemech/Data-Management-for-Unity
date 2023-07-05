using System.Collections;

namespace Data_Management_for_Unity.Runtime.Databases.ValueStorages
{
    public class CollectionStorage<T> : ValueStorage<T> where T : ICollection
    {
        public CollectionStorage(string id, Database database) : base(id, database)
        {
            
        }
    }
}