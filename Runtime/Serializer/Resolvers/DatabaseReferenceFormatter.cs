using System;
using System.Runtime.Serialization;
using Data_Management_for_Unity.Runtime.Objects;
using MessagePack;
using MessagePack.Formatters;

namespace Data_Management_for_Unity.Runtime.Serializer.Resolvers
{
    public class DatabaseReferenceFormatter<T> : IMessagePackFormatter<T>
    {
        public static readonly DatabaseReferenceFormatter<T> Instance = new DatabaseReferenceFormatter<T>();
        
        public void Serialize(ref MessagePackWriter writer, T value, MessagePackSerializerOptions options)
        {
            if(value == null)
            {
                writer.WriteNil();
                return;
            }

            if(value is not DatabaseReference databaseReference)
                throw new ArgumentException("Value is not a DatabaseReference");
            
            writer.Write(databaseReference.Id);
            writer.Write(databaseReference.IsPersistent);
            writer.Write(databaseReference.IsSynchronised);
        }

        public T Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            if(reader.TryReadNil())
            {
                return default;
            }
            
            //create instance
            Type type = typeof(T);
            T obj = (T)FormatterServices.GetUninitializedObject(type);
            
            //manually update values, as they are read-only
            type.GetField("Id").SetValue(obj, reader.ReadString());
            type.GetField("IsPersistent").SetValue(obj, reader.ReadBoolean());
            type.GetField("IsSynchronised").SetValue(obj, reader.ReadBoolean());
            
            return obj;
        }
    }
}