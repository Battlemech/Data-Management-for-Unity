using System;
using System.Collections.Generic;
using Data_Management_for_Unity.Runtime.Serializer;
using MessagePack;

namespace Data_Management_for_Unity.Runtime.Databases.SynchronisedOperations
{
    public class SynchronisedAdd<TCollection, TValue> : CollectionOperation<TCollection, TValue>
        where TCollection : ICollection<TValue>, new()
    {
        //serialize added value
        [Key(4)]
        private readonly byte[] _addedValue;
        [Key(5)]
        private readonly string _addedTypeString;
        
        public SynchronisedAdd(string databaseId, string valueId, byte[] addedValue, Type addedType, bool isSafe, Action<TCollection> onConfirmed) : base(databaseId, valueId, isSafe, onConfirmed)
        {
            _addedValue = addedValue;
            _addedTypeString = addedType.AssemblyQualifiedName;
        }
        
        [SerializationConstructor]
        protected SynchronisedAdd(string databaseId, string valueId, int modCount, bool isSafe, byte[] addedValue, string addedTypeString) : base(databaseId, valueId, isSafe, null)
        {
            ModCount = modCount;
            _addedValue = addedValue;
            _addedTypeString = addedTypeString;
        }

        protected override TCollection PerformAction(TCollection collection)
        {
            //deserialize value to add
            object deserialized = SerializationPCK.Deserialize(_addedValue, Type.GetType(_addedTypeString, true));
            
            //make sure object to add is of expected type
            if (deserialized is not TValue value)
                throw new InvalidCastException($"Expected collection of type {typeof(TCollection)}, but was {deserialized?.GetType()}");
            
            //add element
            collection.Add(value);
            
            //return updated collection
            return collection;
        }
    }
}