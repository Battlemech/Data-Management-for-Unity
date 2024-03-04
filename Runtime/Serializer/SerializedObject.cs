using System;
using MessagePack;

namespace Data_Management_for_Unity.Runtime.Serializer
{
    public struct SerializedObject
    {
        //serialized object
        public readonly ReadOnlyMemory<byte> Bytes;
        public readonly string TypeAsString;
        
        //deserialize type, if necessary
        public Type Type => _type ??= Type.GetType(TypeAsString, true);
        private Type _type;
        
        public SerializedObject(object toSerialize)
        {
            //no object was passed
            if (toSerialize == null)
            {
                Bytes = null;
                TypeAsString = null;
                _type = null;
                return;
            }
            
            //save type
            _type = toSerialize.GetType();
            TypeAsString = _type.AssemblyQualifiedName;

            //todo: directly write into buffer
            Bytes = MessagePackSerializer.Serialize(toSerialize,
                MessagePack.Resolvers.ContractlessStandardResolver.Options);
        }

        public object Deserialize()
        {
            return _type == null ? null : MessagePackSerializer.Deserialize(Type, Bytes);
        }

        public T Deserialize<T>()
        {
            object o = Deserialize();

            return o switch
            {
                T expected => expected,
                null => default,
                _ => throw new ArgumentException("Expected type of " + o.GetType() + 
                                                 ", but got " + typeof(T) + " instead!")
            };
        }
    }
}