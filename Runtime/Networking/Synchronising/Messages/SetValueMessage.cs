using System;
using Data_Management_for_Unity.Runtime.Databases;
using Data_Management_for_Unity.Runtime.Databases.Structs;

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

        public SetValueMessage(ValueReference reference, byte[] value, Type type, int modCount)
        {
            Reference = reference;
            Value = value;
            TypeString = type.AssemblyQualifiedName;
            ModCount = modCount;
        }
        
        public SetValueMessage(string databaseId, string valueId, byte[] value, Type type, int modCount) 
            : this(new ValueReference(databaseId, valueId), value, type, modCount)
        {
            
        }

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