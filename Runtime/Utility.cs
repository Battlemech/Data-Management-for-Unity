using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Data_Management_for_Unity.Runtime.Databases.Structs;
using Data_Management_for_Unity.Runtime.Databases.ValueStorages;
using Data_Management_for_Unity.Runtime.Serializer;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Data_Management_for_Unity.Runtime
{
    public static class Utility
    {
        public static IEnumerator AsIEnumerator(this Task task)
        {
            while (!task.IsCompleted)
            {
                yield return null;
            }

            if (task.IsFaulted)
            {
                ExceptionDispatchInfo.Capture(task.Exception).Throw();
            }
        }

        public static IEnumerator<T> AsIEnumerator<T>(this Task<T> task)
            where T : class
        {
            while (!task.IsCompleted)
            {
                yield return null;
            }

            if (task.IsFaulted)
            {
                ExceptionDispatchInfo.Capture(task.Exception).Throw();
            }

            yield return task.Result;
        }

        public static string GetContent(this IEnumerable enumerable)
        {
            return enumerable.Cast<object>().Aggregate("[",
                (current, value) => current + (value is IEnumerable e ? e.GetContent() : value) + ' ') + ']';
        }
        
        public static byte[] InvokeSafe<T>(this ModifyDelegate<T> modify, byte[] value, Type type, out Type resultType)
        {
            object current = Serialization.Deserialize(value, type);

            return current switch
            {
                T data => Serialization.Serialize(modify.Invoke(data), out resultType),
                null => Serialization.Serialize(modify.Invoke(default), out resultType),
                _ => throw new ArgumentException($"Expected {typeof(T)}, but got {current.GetType()}")
            };
        }
    }
}