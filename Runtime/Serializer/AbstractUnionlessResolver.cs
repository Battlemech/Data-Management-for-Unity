using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using UnityEngine;

namespace Data_Management_for_Unity.Runtime.Serializer
{
    public class AbstractUnionlessResolver : IFormatterResolver
    {
        public static readonly AbstractUnionlessResolver Instance = new AbstractUnionlessResolver();

        private readonly ConcurrentDictionary<Type, object> formatters = new ConcurrentDictionary<Type, object>();

        public IMessagePackFormatter<T> GetFormatter<T>()
        {
            Type type = typeof(T);

            // Try to get the formatter from the cache
            if (formatters.TryGetValue(type, out var formatter))
            {
                return (IMessagePackFormatter<T>)formatter;
            }

            // If the formatter is not in the cache, create it
            if (type.IsAbstract && !type.GetCustomAttributes(typeof(UnionAttribute)).Any())
            {
                /*
                 * abstract classes lacking the union attribute probably can't be tagged with it, since their children have a generic type
                 * -> serialize their actual type and use the standard formatter
                 */
                
                formatter = AbstractUnionlessFormatter<T>.Instance;
            }
            else
            {
                formatter = StandardResolverAllowPrivate.Instance.GetFormatter<T>();
            }

            // Add the formatter to the cache
            formatters.TryAdd(type, formatter);

            return (IMessagePackFormatter<T>)formatter;
        }
    }
}