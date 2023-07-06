using System;
using Data_Management_for_Unity.Runtime.Databases;
using Data_Management_for_Unity.Runtime.Networking.Messaging;

namespace Data_Management_for_Unity.Runtime.Networking.Synchronising.Messages
{
    public class SetValueRequest : ValueRequest
    {
        public readonly ValueReference Reference;
        public readonly byte[] Value;
        public readonly string TypeString;
        public readonly int ModCount;

        public SetValueRequest(string databaseId, string valueId, byte[] value, Type type, int modCount)
        {
            Reference = new ValueReference(databaseId, valueId);
            Value = value;
            TypeString = type.AssemblyQualifiedName;
            ModCount = modCount;
        }

        public Type DeserializeType()
        {
            return Type.GetType(TypeString, true);
        }

        public override SetValueMessage ToMessage()
        {
            return new SetValueMessage(this);
        }
    }

    public class AccessValueReply : Reply
    {
        /// <summary>
        /// Modification count expected by server
        /// </summary>
        public readonly int Expected;
        
        public AccessValueReply(SetValueRequest request, int expected) : base(request)
        {
            Expected = expected;
        }

        /// <summary>
        /// Given the local modification count, returns true if the request was successful, otherwise false
        /// </summary>
        public bool Success(int modCount) => modCount == Expected;
    }
}