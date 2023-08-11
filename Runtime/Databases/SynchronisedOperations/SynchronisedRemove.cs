using System;
using System.Collections.Generic;
using Data_Management_for_Unity.Runtime.Serializer;

namespace Data_Management_for_Unity.Runtime.Databases.SynchronisedOperations
{
    public class SynchronisedRemove<TCollection, TValue> : CollectionOperation<TCollection, TValue>
        where TCollection : ICollection<TValue>, new()
    {
        //serialize removed value
        private readonly byte[] _removedValue;
        private readonly string _removedTypeString;
        
        public SynchronisedRemove(string databaseId, string valueId, byte[] removedValue, Type removedType, bool isSafe, Action<TCollection> onConfirmed) : base(databaseId, valueId, isSafe, onConfirmed)
        {
            _removedValue = removedValue;
            _removedTypeString = removedType.AssemblyQualifiedName;
        }

        protected override TCollection PerformAction(TCollection collection)
        {
            //deserialize value to remove
            object deserialized = Serialization.Deserialize(_removedValue, Type.GetType(_removedTypeString, true));
            
            //make sure object to add is of expected type
            if (deserialized is not TValue value)
                throw new InvalidCastException($"Expected collection of type {typeof(TCollection)}, but was {deserialized?.GetType()}");
            
            //remove element
            collection.Remove(value);
            
            //return updated collection
            return collection;
        }
    }
}