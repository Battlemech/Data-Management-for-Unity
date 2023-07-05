using System;
using Data_Management_for_Unity.Runtime.Databases.Structs;
using Data_Management_for_Unity.Runtime.Networking.Synchronising.Messages;

namespace Data_Management_for_Unity.Runtime.Databases.DelayedOperations
{
    public abstract class DelayedOperation
    {
        public readonly int ModCount;

        protected DelayedOperation(int modCount)
        {
            ModCount = modCount;
        }

        /// <summary>
        /// Repeats the delayed operation
        /// </summary>
        /// <param name="databaseId">Id of database which is processing delayed operation</param>
        /// <param name="valueId">Id of value on database for which delayed operation is being processed</param>
        /// <param name="value">Current value of current value</param>
        /// <param name="type">Current type of current value</param>
        /// <returns>Object which needs to be processed locally and on remote. Example: SetValueMessage</returns>
        public abstract object Invoke(string databaseId, string valueId, byte[] value, Type type);
    }
    
    public class DelayedSet : DelayedOperation
    {
        private readonly byte[] _value;
        private readonly Type _type;
        
        public DelayedSet(byte[] value, Type type, int modCount) : base(modCount)
        {
            _value = value;
            _type = type;
        }
        
        public override object Invoke(string databaseId, string valueId, byte[] value, Type type)
        {
            // Overwrites current value with the one which was attempted to be set earlier
            return new SetValueMessage(databaseId, valueId, _value, _type, ModCount);
        }
    }
    
    public class DelayedModify<T> : DelayedOperation
    {
        private readonly ModifyDelegate<T> _modify;

        public DelayedModify(ModifyDelegate<T> modify, int modCount) : base(modCount)
        {
            _modify = modify;
        }
        
        public override object Invoke(string databaseId, string valueId, byte[] value, Type type)
        {
            //repeat operation with up-to-date value, overwriting old value and type
            value = _modify.InvokeSafe(value, type, out type);

            return new SetValueMessage(databaseId, valueId, value, type, ModCount);
        }
    }
}