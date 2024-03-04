using System;
using MessagePack;
using MessagePack.Resolvers;

namespace Data_Management_for_Unity.Runtime.Serializer
{
    public static class SerializationPCK
    {
        static SerializationPCK()
        {

        }
        
        public static byte[] Serialize<T>(T value)
        {
            //return MessagePackSerializer.Serialize(value, ContractlessStandardResolver.Options);
            return MessagePackSerializer.Serialize(value);
        }

        public static object Deserialize(ReadOnlyMemory<byte> bytes, Type type)
        {
            return MessagePackSerializer.Deserialize(type, bytes);
        }

        public static T Deserialize<T>(ReadOnlyMemory<byte> bytes)
        {
            return MessagePackSerializer.Deserialize<T>(bytes);
        }
    }
}