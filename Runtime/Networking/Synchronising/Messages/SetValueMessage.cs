using System;
using Data_Management_for_Unity.Runtime.Databases;

namespace Data_Management_for_Unity.Runtime.Networking.Synchronising.Messages
{
    public readonly struct SetValueMessage
    {
        public string DatabaseId => Reference.DatabaseId;
        public string ValueId => Reference.ValueId;
        
        public readonly ValueReference Reference;
        public readonly byte[] Value;
        public readonly Type Type;
        public readonly int ModCount;

        public SetValueMessage(SetValueRequest request) : this(request.Reference, request.Value, request.Type, request.ModCount)
        {
            
        }
        
        public SetValueMessage(ValueReference reference, byte[] value, Type type, int modCount)
        {
            Reference = reference;
            Value = value;
            Type = type;
            ModCount = modCount;
        }
    }
}