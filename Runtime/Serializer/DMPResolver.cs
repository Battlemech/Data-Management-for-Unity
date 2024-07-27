using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using Data_Management_for_Unity.Runtime.Databases.ValueStorages;
using Data_Management_for_Unity.Runtime.Objects;
using Data_Management_for_Unity.Runtime.Serializer.Resolvers;
using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using UnityEngine;

namespace Data_Management_for_Unity.Runtime.Serializer
{
    public class DMPResolver : IFormatterResolver
    {
        public static readonly DMPResolver Instance = new DMPResolver();

        private readonly ConcurrentDictionary<Type, object> formatters = new ConcurrentDictionary<Type, object>();

        public IMessagePackFormatter<T> GetFormatter<T>()
        {
            Type type = typeof(T);

            // Try to get the formatter from the cache
            if (formatters.TryGetValue(type, out var formatter))
            {
                return (IMessagePackFormatter<T>)formatter;
            }

            // Create the formatter
            formatter = CreateFormatter<T>(type);
            
            // Add the formatter to the cache
            formatters.TryAdd(type, formatter);

            return (IMessagePackFormatter<T>)formatter;
        }

        private IMessagePackFormatter CreateFormatter<T>(Type type)
        {
            //database references contain many valueStorages, which are ignored during serialization
            if (typeof(DatabaseReference).IsAssignableFrom(type))
            {
                return DatabaseReferenceFormatter<T>.Instance;
            }
            
            //abstract classes lacking the union attribute probably can't be tagged with it, since their children have a generic type.
            //-> serialize their actual type and use the standard formatter
            if (type.IsAbstract && !type.GetCustomAttributes(typeof(UnionAttribute)).Any())
            {
                return AbstractUnionlessFormatter<T>.Instance;
            }
            
            return StandardResolverAllowPrivate.Instance.GetFormatter<T>();
        }
    }
}