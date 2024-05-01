using System;
using System.Buffers;
using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using UnityEngine;

namespace Data_Management_for_Unity.Runtime.Serializer
{
    public class AbstractUnionlessFormatter<T> : IMessagePackFormatter<T>
    {
        public void Serialize(ref MessagePackWriter writer, T value, MessagePackSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNil();
                return;
            }

            //get actual instance type of abstract class
            Type type = value.GetType();
            
            //write type
            writer.Write(type.AssemblyQualifiedName);
            
            //write serialized object
            MessagePackSerializer.Serialize(type, ref writer, value, options);
        }

        public T Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil())
            {
                return default;
            }

            //read type
            string typeName = reader.ReadString();
            
            //ensure type was read
            if (string.IsNullOrEmpty(typeName)) throw new Exception("Failed to read type of " + typeof(T));
            
            //extract type
            Type type = Type.GetType(typeName, true);
            
            //deserialize object
            return (T) MessagePackSerializer.Deserialize(type, ref reader, options);
        }
    }
}