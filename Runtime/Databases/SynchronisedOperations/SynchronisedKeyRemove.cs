using System;
using System.Collections.Generic;
using Data_Management_for_Unity.Runtime.Serializer;
using MessagePack;

namespace Data_Management_for_Unity.Runtime.Databases.SynchronisedOperations
{
    [MessagePackObject]
    public class SynchronisedKeyRemove<TDictionary, TKey, TValue> : CollectionOperation<TDictionary, KeyValuePair<TKey, TValue>> 
        where TDictionary : IDictionary<TKey, TValue>, new()
    {
        //serialize removed value
        [Key(4)]
        private readonly byte[] _removedValue;
        [Key(5)]
        private readonly string _removedTypeString;
        
        public SynchronisedKeyRemove(string databaseId, string valueId, byte[] removedValue, Type removedType, bool isSafe, Action<TDictionary> onConfirmed) : base(databaseId, valueId, isSafe, onConfirmed)
        {
            _removedValue = removedValue;
            _removedTypeString = removedType?.AssemblyQualifiedName;
        }

        [SerializationConstructor]
        public SynchronisedKeyRemove(string databaseId, string valueId, int modCount, bool isSave, byte[] removedValue, string removedTypeString) : base(databaseId, valueId, isSave, null)
        {
            ModCount = modCount;
            _removedValue = removedValue;
            _removedTypeString = removedTypeString;
        }

        protected override TDictionary PerformAction(TDictionary collection)
        {
            //deserialize value to remove
            object deserialized = SerializationPCK.Deserialize(_removedValue, GetType(_removedTypeString));
            
            //make sure object to add is of expected type
            if (deserialized is not TKey value)
                throw new InvalidCastException($"Expected collection of type {typeof(TDictionary)}, but was {deserialized?.GetType()}");
            
            //remove element
            collection.Remove(value);
            
            //return updated collection
            return collection;
        }
    }
}