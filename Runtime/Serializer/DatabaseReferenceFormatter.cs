using System;
using System.Reflection;
using System.Runtime.Serialization;
using Codice.CM.Common.Serialization;
using Data_Management_for_Unity.Runtime.Objects;
using MessagePack;
using MessagePack.Formatters;

namespace Data_Management_for_Unity.Runtime.Serializer
{
    public class DatabaseReferenceFormatter<T> : IMessagePackFormatter<T>
    {
        public static readonly DatabaseReferenceFormatter<T> Instance = new DatabaseReferenceFormatter<T>();
        
        public void Serialize(ref MessagePackWriter writer, T value, MessagePackSerializerOptions options)
        {
            if(value is not DatabaseReference dbr)
                throw new InvalidCastException($"Can only serialize DatabaseReferences, not {typeof(T)!}");
            
            writer.Write(dbr.Id);
            writer.Write(dbr.IsPersistent);
            writer.Write(dbr.IsSynchronised);
        }

        public T Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            //create instance
            Type type = typeof(T);
            T obj = (T) FormatterServices.GetUninitializedObject(type);
            
            //manually update values, as they are read-only
            type.GetField("Id").SetValue(obj, reader.ReadString());
            type.GetField("IsPersistent").SetValue(obj, reader.ReadBoolean());
            type.GetField("IsSynchronised").SetValue(obj, reader.ReadBoolean());

            return obj;
        }
    }
}