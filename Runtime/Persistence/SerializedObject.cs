using System;

namespace Data_Management_for_Unity.Runtime.Persistence
{
    public readonly struct SerializedObject
    {
        public readonly string DatabaseId;
        public readonly string ValueId;
        public readonly byte[] Value;
        public readonly Type Type;
        public readonly int ModCount;

        public SerializedObject(string databaseId, string valueId, byte[] value, Type type, int modCount)
        {
            DatabaseId = databaseId;
            ValueId = valueId;
            Value = value;
            Type = type;
            ModCount = modCount;
        }
    }
}