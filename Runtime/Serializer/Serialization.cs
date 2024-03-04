using System;
using DMP.Utility;
using GroBuf;
using UnityEngine;

namespace Data_Management_for_Unity.Runtime.Serializer
{
    public static class Serialization
    {
        private static readonly GroBuf.Serializer Serializer = new(new AttributeAwareExtractor(),
            options: GroBufOptions.WriteEmptyObjects);

        /// <summary>
        ///     Serializes the object
        /// </summary>
        /// <remarks>
        ///     Make sure that the object is given to the function with its original type,
        ///     or serialization will return a byte array representing null!
        /// </remarks>
        public static byte[] Serialize<T>(T o)
        {
            //Using type parameter to avoid an additional cast and allow the serializer to properly read object type.    
            try
            {
                return Serializer.Serialize(o);
            }
            catch (NotSupportedException)
            {
                throw new NotSupportedException($"Failed to serialize {typeof(T)}");
            }
        }

        public static byte[] Serialize<T>(T o, out Type type)
        {
            type = o?.GetType();
            return Serializer.Serialize(o);
        }

        public static byte[] Serialize(Type type, object o)
        {
            return Serializer.Serialize(type, o);
        }

        public static T Deserialize<T>(byte[] bytes)
        {
            return Serializer.Deserialize<T>(bytes);
        }

        public static object Deserialize(byte[] bytes, Type type)
        {
            return type == null ? default : Serializer.Deserialize(type, bytes);
        }
    }
}