using System;
using MessagePack;

namespace Data_Management_for_Unity.Runtime.Databases.SynchronisedOperations
{
    /// <summary>
    /// Informs peers of data found locally and synchronised it if the data is newer.
    /// Otherwise, discards it.
    /// </summary>
    [MessagePackObject]
    public class SynchronisedInform : SynchronisedOperation
    {
        [Key(3)]
        private readonly byte[] _value;

        [Key(4)]
        private readonly string _typeString;
        
        [Key(5)]
        private readonly int _savedModCount;
        
        public SynchronisedInform(string databaseId, string valueId, byte[] value, Type type, int savedModCount) : base(databaseId, valueId)
        {
            _value = value;
            _typeString = type?.AssemblyQualifiedName;
            _savedModCount = savedModCount;
        }
        
        [SerializationConstructor]
        protected SynchronisedInform(string databaseId, string valueId, int modCount, byte[] value, string typeString, int savedModCount) : base(databaseId, valueId)
        {
            ModCount = modCount;
            _value = value;
            _typeString = typeString;
            _savedModCount = savedModCount;
        }
        
        public override byte[] Repeat(byte[] value, Type type, out Type resultType)
        {
            bool isValid = IsOperationValid(ModCount);

            if (isValid)
            {
                //update modCount on client. IsOperationValid() checks afterward can fail now!
                ModCount = _savedModCount + 1;
                
                //return value
                resultType = Type.GetType(_typeString);
                return _value;
            }
            
            //operation contains outdated data: Don't overwrite current
            resultType = type;
            return value;
        }

        public override byte[] OnRemoteClient(byte[] value, Type type, out Type resultType)
        {
            return Repeat(value, type, out resultType);
        }

        protected override bool IsOperationValid(int expectedModCount)
        {
            //saved modCount is greater: data is newer
            return base.IsOperationValid(expectedModCount) && _savedModCount >= expectedModCount;
        }

        protected internal override bool OnServerValidation(int expectedModCount, out int updatedModCount)
        {
            bool isValid = IsOperationValid(expectedModCount);

            //increment from locally saved modCount if Operation is valid.
            //Otherwise, ensure the modCount is not incremented, allowing this operation to be discarded on failure
            updatedModCount = (isValid ? _savedModCount + 1: expectedModCount);
            
            return isValid;
        }

        public override bool IsSafeOperation()
        {
            return false;
        }

        public override bool DiscardOnFailure()
        {
            return true;
        }

        public override void OnConfirmed(byte[] value, Type type)
        {
            //no action is necessary once the operation is confirmed
        }
    }
}