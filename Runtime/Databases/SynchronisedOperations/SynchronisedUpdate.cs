using System;
using System.Collections.Generic;
using Data_Management_for_Unity.Runtime.Serializer;
using MessagePack;

namespace Data_Management_for_Unity.Runtime.Databases.SynchronisedOperations
{
    public class SynchronisedUpdate<TCollection, TKey, TValue> : CollectionOperation<TCollection, KeyValuePair<TKey, TValue>>
        where TCollection : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, new()
    {
        //serialize key
        [Key(4)]
        private readonly byte[] _updatedKeyValue;
        [Key(5)]
        private readonly string _updatedKeyTypeString;
        
        //serialize value
        [Key(6)]
        private readonly byte[] _updatedValueValue;
        [Key(7)]
        private readonly string _updatedValueTypeString;
        
        public SynchronisedUpdate(string databaseId, string valueId, byte[] updatedKeyValue, Type updatedKeyType, byte[] updatedValueValue, Type updatedValueType, bool isSafe, Action<TCollection> onConfirmed) : base(databaseId, valueId, isSafe, onConfirmed)
        {
            _updatedKeyValue = updatedKeyValue;
            _updatedKeyTypeString = updatedKeyType.AssemblyQualifiedName;
            _updatedValueValue = updatedValueValue;
            _updatedValueTypeString = updatedValueType.AssemblyQualifiedName;
        }
        
        [SerializationConstructor]
        protected SynchronisedUpdate(string databaseId, string valueId, int modCount, bool isSafe, byte[] updatedKeyValue, string updatedKeyTypeString, byte[] updatedValueValue, string updatedValueTypeString) : base(databaseId, valueId, isSafe, null)
        {
            ModCount = modCount;
            _updatedKeyValue = updatedKeyValue;
            _updatedKeyTypeString = updatedKeyTypeString;
            _updatedValueValue = updatedValueValue;
            _updatedValueTypeString = updatedValueTypeString;

            var test = new Dictionary<string, int>();
        }

        protected override TCollection PerformAction(TCollection collection)
        {
            //deserialize key
            Type updatedKeyType = Type.GetType(_updatedKeyTypeString);
            object keyDeserialized = SerializationPCK.Deserialize(_updatedKeyValue, updatedKeyType);
            
            //make sure key is of expected type
            if (keyDeserialized is not TKey key)
                throw new InvalidCastException($"Expected key of type {typeof(TKey)}, but was {keyDeserialized?.GetType()}");
            
            //deserialize value
            Type updatedValueType = Type.GetType(_updatedValueTypeString);
            object valueDeserialized = SerializationPCK.Deserialize(_updatedValueValue, updatedValueType);
            
            //make sure value is of expected type
            if (valueDeserialized is not TValue value)
                throw new InvalidCastException($"Expected value of type {typeof(TValue)}, but was {valueDeserialized?.GetType()}");
            
            //update element
            collection[key] = value;
            
            //return updated collection
            return collection;
        }
    }
}