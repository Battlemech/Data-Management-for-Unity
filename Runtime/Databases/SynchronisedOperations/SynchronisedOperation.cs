using System;
using Data_Management_for_Unity.Runtime.Networking;
using Data_Management_for_Unity.Runtime.Objects;
using Data_Management_for_Unity.Runtime.Serializer;
using MessagePack;
using UnityEngine;

namespace Data_Management_for_Unity.Runtime.Databases.SynchronisedOperations
{
    [MessagePackObject]
    public abstract class SynchronisedOperation
    {
        /// <summary>
        /// Id of the database the operation was performed on
        /// </summary>
        [Key(0)]
        public readonly string DatabaseId;

        /// <summary>
        /// Id of the value the operation was performed on
        /// </summary>
        [Key(1)]
        public readonly string ValueId;
        
        /// <summary>
        /// Expected modificationCount, used to synchronise order of operations
        /// </summary>
        [Key(2)]
        public int ModCount;

        protected SynchronisedOperation(string databaseId, string valueId)
        {
            DatabaseId = databaseId;
            ValueId = valueId;
        }

        /// <summary>
        /// Create a tuple containing the database and value id of value operation was performed on
        /// </summary>
        public ValueReference GetReference()
        {
            return new ValueReference(DatabaseId, ValueId);
        }
        
        /// <summary>
        /// Operation is repeated locally after synchronisation failed initially.
        /// </summary>
        /// <param name="value">Current value</param>
        /// <param name="type">Current type</param>
        /// <param name="resultType">Result type</param>
        /// <returns>Result value</returns>
        /// <remarks>IsOperationValid() can return invalid data after this operation was called! (See Synchronised Inform for more details)</remarks>
        public abstract byte[] Repeat(byte[] value, Type type, out Type resultType);

        /// <summary>
        /// Operation is processed on remote clients.
        /// </summary>
        /// <param name="value">Current value</param>
        /// <param name="type">Current type</param>
        /// <param name="resultType">Result type</param>
        /// <returns>Result value</returns>
        /// <remarks>IsOperationValid() can return invalid data after this operation was called! (See Synchronised Inform for more details)</remarks>
        public abstract byte[] OnRemoteClient(byte[] value, Type type, out Type resultType);

        /// <summary>
        /// Checks if the operation is safe, meaning that inconsistent states will be prevented.
        /// This is ensured by making sure up-to-date data exists on client before executing;
        /// </summary>
        public abstract bool IsSafeOperation();

        /// <summary>
        /// Invoked after a value has been confirmed by the remote.
        /// </summary>
        /// <param name="value">Confirmed value</param>
        /// <param name="type">Confirmed type</param>
        public abstract void OnConfirmed(byte[] value, Type type);

        protected virtual bool IsOperationValid(int expectedModCount)
        {
            return ModCount == expectedModCount;
        }

        protected internal virtual bool OnServerValidation(int expectedModCount, out int updatedModCount)
        {
            bool isValid = IsOperationValid(expectedModCount);

            //per default, modCount is incremented whenever an operation is expected to be executed, either instantly or later
            updatedModCount = (!isValid && DiscardOnFailure()) ? expectedModCount : ( expectedModCount + 1 );

            return isValid;
        }
        
        public virtual bool DiscardOnFailure()
        {
            return false;
        }
        
        protected Type GetType(string typeString)
        {
            return typeString == null ? null : Type.GetType(typeString, true);
        }
    }

    [MessagePackObject]
    public abstract class SynchronisedOperation<T> : SynchronisedOperation
    {
        /// <summary>
        /// Action is invoked locally once operation was confirmed
        /// </summary>
        [IgnoreMember]
        private readonly Action<T> _onConfirmed;

        protected SynchronisedOperation(string databaseId, string valueId, Action<T> onConfirmed) : base(databaseId, valueId)
        {
            _onConfirmed = onConfirmed;
        }

        public override void OnConfirmed(byte[] value, Type type)
        {
            //no confirmation callback exists
            if(_onConfirmed == null) return;

            //deserialize confirmed value
            object confirmed = SerializationPCK.Deserialize(value, type);

            //invoke onConfirmed, depending on deserialized value
            switch (confirmed)
            {
                case T expected:
                    _onConfirmed.Invoke(expected);
                    return;
                case null:
                    _onConfirmed.Invoke(default);
                    return;
                default:
                    throw new ArgumentException($"Expected {typeof(T)}, but got {confirmed.GetType()}");
            }
        }
    }
}