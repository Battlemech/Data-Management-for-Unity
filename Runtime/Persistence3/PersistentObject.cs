using System;
using Data_Management_for_Unity.Runtime.Serializer;

namespace Data_Management_for_Unity.Runtime.Persistence
{
    public readonly struct PersistentObject
    {
        public readonly string DatabaseId;
        public readonly string ValueId;
        public readonly byte[] Value;
        public readonly Type Type;
        public readonly int ModCount;

        public PersistentObject(string databaseId, string valueId, byte[] value, Type type, int modCount)
        {
            DatabaseId = databaseId;
            ValueId = valueId;
            Value = value;
            Type = type;
            ModCount = modCount;
        }
    }
}