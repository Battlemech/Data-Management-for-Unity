using System;
using Data_Management_for_Unity.Runtime.Databases.Structs;

namespace Data_Management_for_Unity.Runtime.Databases.DelayedOperations
{
    public abstract class DelayedOperation
    {
        public readonly int ModCount;

        protected DelayedOperation(int modCount)
        {
            ModCount = modCount;
        }

        public abstract byte[] Invoke(byte[] value, Type type, out Type resultType);
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
        
        // Overwrites current value with the one which was attempted to be set earlier
        public override byte[] Invoke(byte[] value, Type type, out Type resultType)
        {
            resultType = _type;
            return _value;
        }
    }
    
    public class DelayedModify<T> : DelayedOperation
    {
        private readonly ModifyDelegate<T> _modify;

        public DelayedModify(ModifyDelegate<T> modify, int modCount) : base(modCount)
        {
            _modify = modify;
        }

        public override byte[] Invoke(byte[] value, Type type, out Type resultType)
        {
            return _modify.InvokeSafe(value, type, out resultType);
        }
    }
}