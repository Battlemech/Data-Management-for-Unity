using System;
using MessagePack;

namespace Data_Management_for_Unity.Runtime.Serializer
{
    [MessagePackObject]
    public readonly struct SerializedObject
    {
        //serialized object
        [Key(0)]
        private readonly byte[] _bytes;
        [Key(1)]
        private readonly string _typeAsString;
        
        public SerializedObject(object toSerialize)
        {
            if (toSerialize == null) throw new ArgumentNullException(nameof(toSerialize),"Can't extract type of null object!");

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

        [SerializationConstructor]
        public SerializedObject(byte[] bytes, string typeAsString)
        {
            _bytes = bytes;
            _typeAsString = typeAsString;
        }

        public object Deserialize(out Type type)
        {
            type = Type.GetType(_typeAsString, true);
            return SerializationPCK.Deserialize(_bytes, type);
        }
    }
}