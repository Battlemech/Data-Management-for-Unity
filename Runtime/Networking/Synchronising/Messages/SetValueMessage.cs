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
        public readonly string TypeString;
        public readonly int ModCount;

        public SetValueMessage(SetValueRequest request)
        {
            Reference = request.Reference;
            Value = request.Value;
            TypeString = request.TypeString;
            ModCount = request.ModCount;
        }

        public Type GetSerializedType()
        {
            return Type.GetType(TypeString, true);
        }
    }
}