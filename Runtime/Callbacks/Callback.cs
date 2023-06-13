using System;
using System.ComponentModel;
using UnityEngine;

namespace Data_Management_for_Unity.Runtime.Callbacks
{
    public abstract class Callback
    {
        public readonly string Name;

        public Callback(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Invokes the callback
        /// </summary>
        /// <param name="value">Value to invoke with</param>
        /// <returns>False if callback needs to be removed, otherwise true</returns>
        public abstract bool Invoke(object value);

        public abstract bool Invoke(object one, object two);
    }

    public class Callback<T> : Callback
    {
        private readonly Action<T> _onTriggered;
        private readonly bool _removeOnError;
        
        public Callback(Action<T> callback, string name, bool removeOnError) : base(name)
        {
            _onTriggered = callback;
            _removeOnError = removeOnError;
        }

        public override bool Invoke(object value)
        {
            if (value is T data) return Invoke(data);
            throw new ArgumentException($"Expected type {typeof(T)}, but got {value?.GetType()}");
        }

        public override bool Invoke(object one, object two)
        {
            throw new ArgumentException("Can't invoke single callback with two parameters");
        }

        public bool Invoke(T value)
        {
            //try invoke callback
            try
            {
                _onTriggered.Invoke(value);
                return true;
            }
            //callback caused exception
            catch (Exception e)
            {
                //exception wasn't expected: Throw it
                if (!_removeOnError) throw;
                
                //exception was expected. Signal caller that it needs to be removed
                Debug.Log($"Removing callback {Name} because it caused exception: {e}");
                return false;
            }
        }
    }
    
    public class Callback<T1, T2> : Callback
    {
        private readonly Action<T1, T2> _onTriggered;
        private readonly bool _removeOnError;
        
        public Callback(Action<T1, T2> callback, string name, bool removeOnError) : base(name)
        {
            _onTriggered = callback;
            _removeOnError = removeOnError;
        }

        public bool Invoke(T1 one, T2 two)
        {
            //try invoke callback
            try
            {
                _onTriggered.Invoke(one, two);
                return true;
            }
            //callback caused exception
            catch (Exception e)
            {
                //exception wasn't expected: Throw it
                if (!_removeOnError) throw;
                
                //exception was expected. Signal caller that it needs to be removed
                Debug.Log($"Removing callback {Name} because it caused exception: {e}");
                return false;
            }
        }
        
        public override bool Invoke(object value)
        {
            return value switch
            {
                object[] { Length: 2 } array when array[0] is T1 one && array[1] is T2 two => Invoke(one, two),
                Tuple<T1, T2> tuple => Invoke(tuple.Item1, tuple.Item2),
                _ => throw new ArgumentException($"Expected types {typeof(T1)},{typeof(T2)}, but got {value?.GetType()}")
            };
        }

        public override bool Invoke(object one, object two)
        {
            if (one is T1 a && two is T2 b) return Invoke(a, b);
            throw new ArgumentException($"Expected types {typeof(T1)},{typeof(T2)}, but got {one?.GetType()} and {two?.GetType()}");
        }
    }
}