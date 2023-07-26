using System;
using System.Collections.Generic;
using Data_Management_for_Unity.Runtime.Serializer;

namespace Data_Management_for_Unity.Runtime.Databases.SynchronisedOperations
{
    public class SynchronisedKeyRemove<TDictionary, TKey, TValue> : CollectionOperation<TDictionary, KeyValuePair<TKey, TValue>> 
        where TDictionary : IDictionary<TKey, TValue>, new()
    {
        //serialize removed value
        private readonly byte[] _removedValue;
        private readonly string _removedTypeString;
        
        public SynchronisedKeyRemove(string databaseId, string valueId, byte[] removedValue, Type removedType, bool isSafe) : base(databaseId, valueId, isSafe)
        {
            _removedValue = removedValue;
            _removedTypeString = removedType.AssemblyQualifiedName;
        }

        protected override TDictionary PerformAction(TDictionary collection)
        {
            //deserialize value to remove
            object deserialized = Serialization.Deserialize(_removedValue, Type.GetType(_removedTypeString, true));
            
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