using System;
using Data_Management_for_Unity.Runtime.Databases;
using Data_Management_for_Unity.Runtime.Networking.Messaging;

namespace Data_Management_for_Unity.Runtime.Networking.Synchronising.Messages
{
    public class SetValueRequest : Request
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
    }

    public class SetValueReply : Reply
    {
        /// <summary>
        /// Modification count expected by server
        /// </summary>
        public readonly int Expected;
        
        public SetValueReply(SetValueRequest request, int expected) : base(request)
        {
            Expected = expected;
        }

        /// <summary>
        /// Request was successful if local mod count matched servers remote mod count
        /// </summary>
        /// <param name="local"></param>
        /// <returns></returns>
        public bool Success(int local) => local == Expected;
    }
}