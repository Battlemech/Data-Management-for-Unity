using System;
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
}