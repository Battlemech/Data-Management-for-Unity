using System;
using System.Threading.Tasks;
using Data_Management_for_Unity.Runtime.Databases.Structs;
using Data_Management_for_Unity.Runtime.Networking.Synchronising.Client;
using Data_Management_for_Unity.Runtime.Networking.Synchronising.Messages;
using Data_Management_for_Unity.Runtime.Serializer;
using DMP.Utility;

namespace Data_Management_for_Unity.Runtime.Databases.SynchronisedOperations
{
    public class SynchronisedModify<T> : SynchronisedOperation
    {
        [PreventSerialization]
        private readonly ModifyDelegate<T> _modify;
        
        //saves value and type resulting from operation to allow synchronising result on remote
        private byte[] _value;
        private string _typeString;

        //safe operations are only executed if the client is certain that it has up-to-date data
        private readonly bool _isSafe;
        
        public SynchronisedModify(string databaseId, string valueId, byte[] value, Type type, ModifyDelegate<T> modify, bool isSafe) : base(databaseId, valueId)
        {
            _modify = modify;

            //save value to allow setting it on remote
            _value = value;
            _typeString = type.AssemblyQualifiedName;
            
            //save "safe" attribute
            _isSafe = isSafe;
        }

        public override byte[] Repeat(byte[] value, Type type, out Type resultType)
        {
            //updates cached value and type since operation had to be repeated
            _value = _modify.InvokeSafe(value, type, out resultType);
            
            //update serialized type
            _typeString = resultType.AssemblyQualifiedName;

            //return value and type
            return _value;
        }

        public override byte[] OnRemote(byte[] value, Type type, out Type resultType)
        {
            //deserialize type
            resultType = Type.GetType(_typeString, true);
            return _value;
        }

        public override bool IsSafeOperation()
        {
            return _isSafe;
        }
    }
}