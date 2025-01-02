using System;
using Data_Management_for_Unity.Runtime.Databases.Structs;
using MessagePack;


namespace Data_Management_for_Unity.Runtime.Databases.SynchronisedOperations
{
    [MessagePackObject]
    public class SynchronisedModify<T> : SynchronisedOperation<T>
    {
        [IgnoreMember]
        private readonly ModifyDelegate<T> _modify;

        //saves value and type resulting from operation to allow synchronising result on remote
        [Key(3)]
        private byte[] _value;
        [Key(4)]
        private string _typeString;

        //safe operations are only executed if the client is certain that it has up-to-date data
        [Key(5)]
        private readonly bool _isSafe;
        
        public SynchronisedModify(string databaseId, string valueId, byte[] value, Type type, ModifyDelegate<T> modify, bool isSafe, Action<T> onConfirmed) 
            : base(databaseId, valueId, onConfirmed)
        {
            _modify = modify;

            //save value to allow setting it on remote
            _value = value;
            _typeString = type?.AssemblyQualifiedName;
            
            //save "safe" attribute
            _isSafe = isSafe;
        }

        [SerializationConstructor]
        protected SynchronisedModify(string databaseId, string valueId, int modCount, byte[] value, string typeString, bool isSafe):
            base(databaseId, valueId, null)
        {
            ModCount = modCount;
            _value = value;
            _typeString = typeString;
            _isSafe = isSafe;
        }

        public override byte[] Repeat(byte[] value, Type type, out Type resultType)
        {
            //updates cached value and type since operation had to be repeated
            _value = _modify.InvokeSafe(value, type, out resultType);
            
            //update serialized type
            _typeString = resultType?.AssemblyQualifiedName;

            //return value and type
            return _value;
        }

        public override byte[] OnRemote(byte[] value, Type type, out Type resultType)
        {
            //deserialize type
            resultType = GetType(_typeString);
            return _value;
        }

        public override bool IsSafeOperation()
        {
            return _isSafe;
        }
    }
}