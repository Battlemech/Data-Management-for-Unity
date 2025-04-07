using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data_Management_for_Unity.Runtime.Serializer;
using JetBrains.Annotations;

namespace Data_Management_for_Unity.Runtime.Databases.ValueStorages
{
    public static class Utility
    {
        //todo: safe collection operations instantly modify the value ...
        public static Task Add<TCollection, TValue>(this ValueStorage<TCollection> valueStorage, TValue toAdd, bool safe=false, Action<TCollection> onConfirmed=null)
            where TCollection : ICollection<TValue>, new()
        {
            //serialize added value
            byte[] addedValue = SerializationPCK.Serialize(toAdd, out Type addedType);

            //serialize entire collection
            byte[] collectionValue;
            Type collectionType;
            
            lock (valueStorage.Id)
            {
                if (!safe)
                {
                    //init collection if necessary
                    valueStorage.Data ??= new TCollection();
                
                    //add value to collection
                    valueStorage.Data.Add(toAdd);   
                }

                //serialize collection
                collectionValue = SerializationPCK.Serialize(valueStorage.Data, out collectionType);
            }
            
            //process add internally
            return valueStorage.Database.OnAdd<TCollection, TValue>(valueStorage.Id, collectionValue, collectionType, addedValue, addedType, safe, onConfirmed);
        }

        public static Task Remove<TCollection, TValue>(this ValueStorage<TCollection> valueStorage, TValue toRemove, bool safe=false, Action<TCollection> onConfirmed=null)
            where TCollection : ICollection<TValue>, new()
        {
            //serialize removed value
            byte[] removedValue = SerializationPCK.Serialize(toRemove, out Type removedType);

            //serialize entire collection
            byte[] collectionValue;
            Type collectionType;
            
            lock (valueStorage.Id)
            {
                if (!safe)
                {
                    //init collection if necessary
                    valueStorage.Data ??= new TCollection();
                
                    //remove value from collection
                    valueStorage.Data.Remove(toRemove);   
                }

                //serialize collection
                collectionValue = SerializationPCK.Serialize(valueStorage.Data, out collectionType);
            }

            //process remove internally
            return valueStorage.Database.OnRemove<TCollection, TValue>(valueStorage.Id, collectionValue, collectionType, removedValue, removedType, safe, onConfirmed);
        }

        public static Task Add<TCollection, TKey, TValue>(this ValueStorage<TCollection> valueStorage, TKey key, TValue value, bool safe=false, Action<TCollection> onConfirmed=null)
            where TCollection : IDictionary<TKey, TValue>, new()
        {
            return valueStorage.Add(new KeyValuePair<TKey, TValue>(key, value), safe, onConfirmed);
        }

        public static Task RemoveKey<TCollection, TKey, TValue>(this ValueStorage<TCollection> valueStorage, TKey toRemove, bool safe=false, Action<TCollection> onConfirmed=null)
            where TCollection : IDictionary<TKey, TValue>, new()
        {
            //serialize removed key
            byte[] removedValue = SerializationPCK.Serialize(toRemove, out Type removedType);

            //serialize entire collection
            byte[] collectionValue;
            Type collectionType;
            
            lock (valueStorage.Id)
            {
                if (!safe)
                {
                    //init collection if necessary
                    valueStorage.Data ??= new TCollection();
                
                    //remove value from collection
                    valueStorage.Data.Remove(toRemove);   
                }

                //serialize collection
                collectionValue = SerializationPCK.Serialize(valueStorage.Data, out collectionType);
            }

            //process remove internally
            return valueStorage.Database.OnRemoveKey<TCollection, TKey, TValue>(valueStorage.Id, collectionValue, collectionType, removedValue, removedType, safe, onConfirmed);
        }

        public static bool TryGetValue<TCollection, TKey, TValue>(this ValueStorage<TCollection> valueStorage, TKey key, out TValue value)
            where TCollection : IDictionary<TKey, TValue>
        {
            //allow value to be modified in lambda
            TValue result = default;
            
            //try getting value
            bool success = valueStorage.BlockingGet((collection => collection != null && collection.TryGetValue(key, out result)));

            //save result in out parameter
            value = result;

            return success;
        }
        
        public static Task Update<TCollection, TKey, TValue>(this ValueStorage<TCollection> valueStorage, TKey key, TValue value, bool safe=false, Action<TCollection> onConfirmed=null)
            where TCollection : IDictionary<TKey, TValue>, new()
        {
            //serialize key and value
            byte[] updatedKey = SerializationPCK.Serialize(key, out Type updatedKeyType);
            byte[] updatedValue = SerializationPCK.Serialize(value, out Type updatedType);
            
            //serialize entire collection
            byte[] collectionValue;
            Type collectionType;
            
            lock (valueStorage.Id)
            {
                if (!safe)
                {
                    //init collection if necessary
                    valueStorage.Data ??= new TCollection();
                
                    //update value in collection
                    valueStorage.Data[key] = value;   
                }

                //serialize collection
                collectionValue = SerializationPCK.Serialize(valueStorage.Data, out collectionType);
            }
            
            //process update internally
            return valueStorage.Database.OnUpdate<TCollection, TKey, TValue>(valueStorage.Id, collectionValue, collectionType, updatedKey, updatedKeyType, updatedValue, updatedType, safe, onConfirmed);
        }

        public static bool Contains<TCollection, TValue>(this ValueStorage<TCollection> valueStorage, TValue value)
            where TCollection : ICollection<TValue>
        {
            return valueStorage.BlockingGet((values => values != null && values.Contains(value)));
        }
        
        /// <summary>
        /// Iterate through all elements of the collection
        /// </summary>
        public static void ForEach<TCollection, TValue>(this ValueStorage<TCollection> valueStorage, Action<TValue> element)
            where TCollection : ICollection<TValue>, new()
        {
            valueStorage.BlockingGet((collection =>
            {
                if(collection == null) return;
                
                foreach (var value in collection)
                {
                    element.Invoke(value);
                }
            }));
        }

        public static bool IsNull<TValue>(this ValueStorage<TValue> valueStorage)
        {
            return valueStorage.BlockingGet((value => value == null));
        }
        
        public static bool IsNullOrEmpty<TCollection>(this ValueStorage<TCollection> valueStorage)
            where TCollection : ICollection
        {
            return valueStorage.BlockingGet((collection => collection == null || collection.Count == 0));
        }
        
        public static IEnumerable<TResult> Select<T, TResult>(
            this ValueStorage<List<T>> valueStorage,
            Func<T, TResult> selector)
        {
            return valueStorage.BlockingGet((list => list?.Select(selector))) ?? Enumerable.Empty<TResult>();
        }
        
        public static IEnumerable<T> Where<T>(
            this ValueStorage<List<T>> valueStorage,
            Func<T, bool> predicate)
        {
            return valueStorage.BlockingGet(list => list?.Where(predicate)) ?? Enumerable.Empty<T>();
        }


        public static IEnumerable<TResult> SelectMany<T, TResult>(
            this ValueStorage<List<T>> valueStorage,
            Func<T, IEnumerable<TResult>> selector)
        {
            return valueStorage.BlockingGet(list => list?.SelectMany(selector)) ?? Enumerable.Empty<TResult>();
        }
        
        public static IEnumerable<TResult> OfType<T, TResult>(
            this ValueStorage<List<T>> valueStorage)
        {
            return valueStorage.BlockingGet((list => list?.OfType<TResult>())) ?? Enumerable.Empty<TResult>();
        }
        
        public static int Count<TCollection>(this ValueStorage<TCollection> valueStorage)
            where TCollection : ICollection
        {
            return valueStorage.BlockingGet((collection => collection == null ? 0 : collection.Count));
        }
        
        public static List<T> Copy<T>(this ValueStorage<List<T>> valueStorage)
        {
            return valueStorage.BlockingGet(collection => collection == null ? null : new List<T>(collection));
        }

    }
}