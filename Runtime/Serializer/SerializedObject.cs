using System;
using MessagePack;

namespace Data_Management_for_Unity.Runtime.Serializer
{
    public readonly struct SerializedObject
    {
        //serialized object
        private readonly byte[] _bytes;
        private readonly string _typeAsString;
        
        public SerializedObject(object toSerialize)
        {
            if (toSerialize == null) throw new ArgumentException("Can't extract type of null object!");

            Type type = toSerialize.GetType();
            
            //serialize type
            _typeAsString = type.AssemblyQualifiedName;
            
            //serialize object
            _bytes = SerializationPCK.Serialize(toSerialize, type);
        }

        public SerializedObject(object toSerialize, Type type)
        {
            //serialize type
            _typeAsString = type.AssemblyQualifiedName;
            
            //serialize object
            _bytes = SerializationPCK.Serialize(toSerialize, type);
        }

        public object Deserialize(out Type type)
        {
            type = Type.GetType(_typeAsString, true);
            return SerializationPCK.Deserialize(_bytes, type);
        }
    }
}