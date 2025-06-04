using System;
using Data_Management_for_Unity.Runtime.Databases;
using Data_Management_for_Unity.Runtime.Networking.Synchronising.Client;
using MessagePack;

namespace Data_Management_for_Unity.Runtime.Objects
{
    [MessagePackObject]
    public class DatabaseReference
    {
        //values are serialised with custom serializer
        public readonly string Id;
        public readonly bool IsSynchronised;
        public readonly bool IsPersistent;

        //ignored by serialisation
        [IgnoreMember]
        private Database _database;

        public DatabaseReference(bool isSynchronised = true, bool isPersistent = true)
        {
            //group default objects by class name
            Id = $"{GetType().Name}/{Guid.NewGuid()}";
            IsSynchronised = isSynchronised;
            IsPersistent = isPersistent;
            _database = null;
        }
        
        [SerializationConstructor]
        public DatabaseReference(string id, bool isSynchronised = true, bool isPersistent = true)
        {
            Id = id;
            IsSynchronised = isSynchronised;
            IsPersistent = isPersistent;
            _database = null;
        }

        public Database GetDatabase()
        {
            //caching
            if (_database != null) return _database;
            //user feedback on invalid operations
            if (SynchronisedClient.Instance == null)
                throw new InvalidOperationException("Databases must be managed by a local synchronised client!");
            //cache newly retrieved database
            return _database = SynchronisedClient.Instance.GetDatabase(Id, IsSynchronised, IsPersistent);
        }
        
        public virtual void Delete()
        {
            //start deletion process
            GetDatabase().Delete();
            
            //clear local reference
            _database = null;
        }

        protected bool Equals(DatabaseReference other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is DatabaseReference other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (Id != null ? Id.GetHashCode() : 0);
        }
        
        public int RemoveCallbacks(string name=null, bool mainThread=true)
        {
            return GetDatabase().RemoveCallbacks(name, mainThread);
        }
        
        /// <summary>
        /// Shares the referenced database in the network
        /// </summary>
        /// <param name="recursive">Also share all databases referenced in the database itself</param>
        public void ShareInNetwork(bool recursive)
        {
            GetDatabase().ShareInNetwork(recursive);
        }
    }
}