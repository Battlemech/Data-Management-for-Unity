using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Debug = UnityEngine.Debug;

namespace Data_Management_for_Unity.Runtime.Callbacks
{
    public class CallbackHandler<TKey>
    {
        private readonly Dictionary<TKey, List<Callback>> _callbacks = new();

        /// <summary>
        /// Adds a callback.
        /// </summary>
        /// <param name="key">Key of the callback function</param>
        /// <param name="callback">Action invoked when callback is triggered</param>
        /// <param name="name">Name of the callback</param>
        /// <param name="unique">True if callbacks with duplicate names must be prevented</param>
        /// <param name="removeOnError">True if the callbacks must be removed on error</param>
        /// <typeparam name="T">Expected type of object in callback</typeparam>
        /// <returns>True if the callback was added, false if the unique parameter could not be met</returns>
        public bool AddCallback<T>(TKey key, Action<T> callback, string name = "", bool unique = false,
            bool removeOnError = false)
        {
            //make sure no other thread is modifying callbacks
            lock (_callbacks)
            {
                //if callback list exists
                if (_callbacks.TryGetValue(key, out var callbacks))
                {
                    //prevent duplicate callbacks with unique=true parameter
                    if (unique && callbacks.Any((c => c.Name == name))) return false;
                }
                else 
                {
                    //init list
                    callbacks = new List<Callback>();
                    _callbacks.Add(key, callbacks);
                }
                
                callbacks.Add(new Callback<T>(callback, name, removeOnError));
            }

            return true;
        }

        /// <summary>
        /// Gets the number of callbacks
        /// </summary>
        /// <param name="key">Key of callbacks</param>
        /// <param name="name">Required name of callbacks, if any</param>
        /// <returns>Number of callbacks matching criterion</returns>
        public int GetCallbackCount(TKey key, string name=null)
        {
            lock (_callbacks)
            {
                //no callbacks matching key
                if (!_callbacks.TryGetValue(key, out List<Callback> callbacks)) return 0;

                //return all callbacks, or callbacks with specified name if required
                return name == null ? callbacks.Count : callbacks.Where((c => c.Name == name)).Count();
            }
        }

        /// <summary>
        /// Removes callbacks
        /// </summary>
        /// <param name="key">Key of callbacks</param>
        /// <param name="name">Required name of callbacks, if any</param>
        /// <returns>Number of callbacks removed</returns>
        public int RemoveCallbacks(TKey key, string name=null)
        {
            lock (_callbacks)
            {
                //no callbacks to remove
                if (!_callbacks.TryGetValue(key, out List<Callback> callbacks)) return 0;
                
                //remove callbacks with specified name
                if (name != null) return callbacks.RemoveAll((c => c.Name == name));
  
                //clear all callbacks, no matter what they are called
                int count = callbacks.Count;
                callbacks.Clear();
                return count;
            }
        }

        /// <summary>
        /// Invokes callbacks
        /// </summary>
        /// <param name="key">Key of callbacks</param>
        /// <param name="value">Value of objects to invoke callbacks with</param>
        /// <param name="name">Required name of callbacks to be invoked, if any</param>
        /// <returns>Number of invoked callbacks</returns>
        public int InvokeCallbacks(TKey key, object value, string name=null)
        {
            lock (_callbacks)
            {
                //no callbacks to invoke
                if(!_callbacks.TryGetValue(key, out List<Callback> callbacks)) return 0;

                //filter callbacks with expected name
                List<Callback> matchingCallbacks =
                    //no required name specified: Invoke all callbacks
                    name == null ? new List<Callback>(callbacks)
                        //get callbacks with expected name
                        : callbacks.Where((callback => callback.Name == name)).ToList();

                //copy list to allow modifying origin
                foreach (var callback in matchingCallbacks.Where(callback => !callback.Invoke(value)))
                {
                    //Callback caused error and needs to be removed
                    callbacks.Remove(callback);
                }

                return matchingCallbacks.Count;
            }
        }
    }
}