using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Data_Management_for_Unity.Runtime.Networking.Synchronising.Client;
using Data_Management_for_Unity.Runtime.Networking.Synchronising.Messages;
using MessagePack;
using MessagePack.Formatters;
using UnityEngine;

namespace Data_Management_for_Unity.Runtime.Databases.SynchronisedOperations
{
    [MessagePackObject]
    public class SynchronisedSet<T> : SynchronisedOperation<T>
    {
        //saves value and type resulting from operation to allow synchronising result on remote
        [Key(3)]
        private readonly byte[] _value;
        [Key(4)] 
        private readonly string _typeString;
        
        public SynchronisedSet(string databaseId, string valueId, byte[] value, Type type, Action<T> onConfirmed) : base(databaseId, valueId, onConfirmed)
        {
            _value = value;
            _typeString = type.AssemblyQualifiedName;
        }

        [SerializationConstructor]
        protected SynchronisedSet(string databaseId, string valueId, int modCount, byte[] value, string typeString) : base(databaseId, valueId, null)
        {
            ModCount = modCount;
            _value = value;
            _typeString = typeString;
        }

        public override byte[] Repeat(byte[] value, Type type, out Type resultType)
        {
            //no action necessary, set overwrites previous value
            resultType = Type.GetType(_typeString, true);
            return _value;
        }

        public override byte[] OnRemote(byte[] value, Type type, out Type resultType)
        {
            //no action necessary, set overwrites previous value
            resultType = Type.GetType(_typeString, true);
            return _value;
        }

        public override bool IsSafeOperation()
        {
            return false;
        }
    }
}