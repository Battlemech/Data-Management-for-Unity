using System;
using Data_Management_for_Unity.Runtime.Databases;
using Data_Management_for_Unity.Runtime.Networking.Synchronising.Client;
using MessagePack;

namespace Data_Management_for_Unity.Runtime.Objects
{
    /// <summary>
    /// Synchronised its values automatically between clients. All of its properties must be ValueStorages,
    /// remaining ones are ignored by the serializer.
    /// </summary>
    [MessagePackObject]
    public abstract class SynchronisedObject
    {
        [Key(0)]
        public readonly string Id;
        
        [Key(1)]
        public readonly bool IsPersistent;
        
        [IgnoreMember]
        private Database _database;

        //new objects with random ids never exist before
        protected SynchronisedObject(bool isPersistent = false) : this(Guid.NewGuid().ToString(), isPersistent, false)
        {
            
        }

        
        protected SynchronisedObject(string id, bool isPersistent, bool getExisting)
        {
            Id = id;
            IsPersistent = isPersistent;
            _database = getExisting 
                ? SynchronisedClient.Instance.GetDatabase(Id, true)
                : new Database(id, isPersistent, true);
        }

        protected Database GetDatabase()
        {
            if (_database != null) return _database;

            if (SynchronisedClient.Instance == null)
                throw new InvalidOperationException("Synchronised object must be managed by a local SynchronisedClient");

            _database = SynchronisedClient.Instance.GetDatabase(Id, true, IsPersistent);

            return _database;
        }

        public void Delete()
        {
            //start deletion process
            GetDatabase().Delete();
            
            //clear local reference
            _database = null;
        }

        protected bool Equals(SynchronisedObject other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is SynchronisedObject other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (Id != null ? Id.GetHashCode() : 0);
        }
    }
}