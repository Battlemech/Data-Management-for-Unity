using System;
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
        
        public IMessagePackFormatter<T> GetFormatter<T>()
        {
            Type type = typeof(T);
            
            //class is abstract and lacks Union attribute -> Parent classes are probably generic and can't be annotated
            if (type.IsAbstract && type.GetCustomAttribute<UnionAttribute>() == null)
            {
                //todo: save statically instead of creating new instance every time?
                return new AbstractUnionlessFormatter<T>();
            }
            
            //return default formatter
            return StandardResolverAllowPrivate.Instance.GetFormatter<T>();
        }
    }
}