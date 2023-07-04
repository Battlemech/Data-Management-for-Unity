using System;

namespace Data_Management_for_Unity.Runtime.Databases
{
    public readonly struct ConfirmedValue
    {
        public readonly byte[] Value;
        public readonly Type Type;
        public readonly int ModCount;

        public ConfirmedValue(byte[] value, Type type, int modCount)
        {
            Value = value;
            Type = type;
            ModCount = modCount;
        }
    }
}