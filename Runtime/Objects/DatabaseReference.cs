using System;
using Data_Management_for_Unity.Runtime.Databases;
using Data_Management_for_Unity.Runtime.Networking.Synchronising.Client;
using MessagePack;

namespace Data_Management_for_Unity.Runtime.Objects
{
    /// <summary>
    /// Allows referencing a database across clients
    /// </summary>
    [MessagePackObject]
    public abstract class DatabaseReference
    {
        [Key(0)]
        public readonly string Id;

        [Key(1)]
        public readonly bool IsPersistent;

        [Key(2)]
        public readonly bool IsSynchronised;

        [IgnoreMember]
        private Database _database;

        protected DatabaseReference(bool isPersistent = false) : this(Guid.NewGuid().ToString(), isPersistent, true) 
        {
            
        }
        
        protected DatabaseReference(string id, bool isPersistent, bool isSynchronised)
        {
            Id = id;
            IsPersistent = isPersistent;
            IsSynchronised = isSynchronised;
        }

        protected Database GetDatabase()
        {
            if (_database != null) return _database;

            return _database = DatabaseManager.Instance.GetDatabase(Id, IsSynchronised, IsPersistent);
        }

        protected bool Equals(DatabaseReference other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DatabaseReference)obj);
        }

        public override int GetHashCode()
        {
            return (Id != null ? Id.GetHashCode() : 0);
        }

        /// <summary>
        /// Manually loads the database object for a referenced client.
        /// Primarily used to simulate synchronisation of database objects from multiple synchronised clients
        /// </summary>
        public void OverwriteDatabase(SynchronisedClient client)
        {
            _database = client.GetDatabase(Id, IsSynchronised, IsPersistent);
        }
    }
}