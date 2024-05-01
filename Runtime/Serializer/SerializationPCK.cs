using System;
using System.Buffers;
using MessagePack;
using MessagePack.Resolvers;

namespace Data_Management_for_Unity.Runtime.Serializer
{
    public static class SerializationPCK
    {
        static SerializationPCK()
        {
            //allow serializing private values per default
            MessagePackSerializer.DefaultOptions = MessagePackSerializerOptions.Standard
                .WithResolver(AbstractUnionlessResolver.Instance);
        }

        public static byte[] Serialize<T>(T o)
        {
            return MessagePackSerializer.Serialize(o);
        }
        
        public static byte[] Serialize(object o, out Type type)
        {
            type = o?.GetType();
            return Serialize(o, type);
        }
        
        public static byte[] Serialize(object o, Type type)
        {
            return MessagePackSerializer.Serialize(type, o);
        }

        public static object Deserialize(ReadOnlyMemory<byte> bytes, Type type)
        {
            return type == null ? default : MessagePackSerializer.Deserialize(type, bytes);
        }
        
        /// <summary>
        /// Deserialize bytes into object of type T
        /// </summary>
        /// <remarks>Ensure that T equals the exact type, not abstract or base type of the serialized object!</remarks>
        public static T Deserialize<T>(ReadOnlyMemory<byte> bytes)
        {
            return MessagePackSerializer.Deserialize<T>(bytes);
        }
        
        /// <summary>
        /// Deserialize bytes into object of type T
        /// </summary>
        /// <remarks>Ensure that T equals the exact type, not abstract or base type of the serialized object!</remarks>
        public static T Deserialize<T>(byte[] bytes)
        {
            return Deserialize<T>(new ReadOnlyMemory<byte>(bytes));
        }

    }
}