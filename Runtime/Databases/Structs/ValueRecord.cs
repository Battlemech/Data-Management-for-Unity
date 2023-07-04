using System;

namespace Data_Management_for_Unity.Runtime.Databases.Structs
{
    public readonly struct ValueRecord
    {
        public readonly byte[] Value;
        public readonly Type Type;
        public readonly int ModCount;

        public ValueRecord(byte[] value, Type type, int modCount)
        {
            Value = value;
            Type = type;
            ModCount = modCount;
        }
    }
}