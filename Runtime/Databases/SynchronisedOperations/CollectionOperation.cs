using System;
using System.Collections.Generic;
using Data_Management_for_Unity.Runtime.Serializer;
using MessagePack;

namespace Data_Management_for_Unity.Runtime.Databases.SynchronisedOperations
{
    [MessagePackObject]
    public abstract class CollectionOperation<TCollection, TValue> : SynchronisedOperation<TCollection>
        where TCollection : ICollection<TValue>, new()
    {
        [Key(3)]
        private readonly bool _isSafeOperation;
        
        protected CollectionOperation(string databaseId, string valueId, bool isSafe, Action<TCollection> onConfirmed) : base(databaseId, valueId, onConfirmed)
        {
            _isSafeOperation = isSafe;
        }

        /// <summary>
        /// Collection operation performs operation on collection
        /// </summary>
        protected abstract TCollection PerformAction(TCollection collection);

        public override byte[] Repeat(byte[] value, Type type, out Type resultType)
        {
            //init out parameter with this type of operation
            resultType = typeof(TCollection);
            
            //if collection wasn't initialized
            if (value == null || type == null)
            {
                //initialize collection and invoke operation
                return SerializationPCK.Serialize(PerformAction(new TCollection()));
            }
            
            //deserialize current collection
            object deserialized = SerializationPCK.Deserialize(value, type);

            //make sure current collection is of expected type
            if (deserialized is not TCollection collection)
                throw new InvalidCastException($"Expected collection of type {typeof(TCollection)}, but was {deserialized?.GetType()}");
            
            //perform operation on collection
            return SerializationPCK.Serialize(PerformAction(collection));
        }

        public override byte[] OnRemoteClient(byte[] value, Type type, out Type resultType)
        {
            //collection operation is the same if repeated locally or if performed on remote
            return Repeat(value, type, out resultType);
        }

        public override bool IsSafeOperation()
        {
            return _isSafeOperation;
        }
    }
}