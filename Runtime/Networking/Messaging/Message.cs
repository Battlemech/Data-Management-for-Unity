using System;
using Data_Management_for_Unity.Runtime.Serializer;

namespace Data_Management_for_Unity.Runtime.Networking.Messaging
{
    public readonly struct Message
    {
        private readonly string _serializedType;
        private readonly byte[] _value;

        private Message(string serializedType, byte[] value)
        {
            _serializedType = serializedType;
            _value = value;
        }

        public object Deserialize(out Type type)
        {
            type = Type.GetType(_serializedType, true);
            return Serialization.Deserialize(_value, type);
        }

        public static Message Create<T>(T data)
        {
            if (data == null) throw new ArgumentNullException();

            Type type = data.GetType();
            
            return new Message(type.AssemblyQualifiedName, Serialization.Serialize(type, data));
        }
    }
}