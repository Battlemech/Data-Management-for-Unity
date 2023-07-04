using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using UnityEngine;

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

        public static string GetContent<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.Aggregate($"[{typeof(T)}]: ", (current, t) => current + t);
        }
    }
}