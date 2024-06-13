using System;
using System.Reflection;
using System.Runtime.Serialization;
using Data_Management_for_Unity.Runtime.Objects;
using MessagePack;
using MessagePack.Formatters;

namespace Data_Management_for_Unity.Runtime.Serializer
{
    public class SynchronisedObjectFormatter<T> : IMessagePackFormatter<T>
    {
        public static readonly SynchronisedObjectFormatter<T> Instance = new SynchronisedObjectFormatter<T>();
        
        public void Serialize(ref MessagePackWriter writer, T value, MessagePackSerializerOptions options)
        {
            if(value is not SynchronisedObject so)
                throw new InvalidCastException($"Can only serialize SynchronisedObjects, not {typeof(T)!}");
            
            writer.Write(so.Id);
        }

        public T Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            //create instance
            T obj = (T) FormatterServices.GetUninitializedObject(typeof(T));
            
            //manually update id, as it is read-only
            typeof(SynchronisedObject).GetField("Id").SetValue(obj, reader.ReadString());

            return obj;
        }
    }
}