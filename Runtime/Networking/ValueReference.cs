using System;

namespace Data_Management_for_Unity.Runtime.Networking
{
    public readonly struct ValueReference
    {
        public readonly string DatabaseId;
        public readonly string ValueId;

        public ValueReference(string databaseId, string valueId)
        {
            DatabaseId = databaseId;
            ValueId = valueId;
        }
        
        public bool Equals(ValueReference other)
        {
            return DatabaseId == other.DatabaseId && ValueId == other.ValueId;
        }

        public override bool Equals(object obj)
        {
            return obj is ValueReference other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(DatabaseId, ValueId);
        }
    }
}