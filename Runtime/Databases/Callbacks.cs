using System;
using Data_Management_for_Unity.Runtime.Callbacks;

namespace Data_Management_for_Unity.Runtime.Databases
{
    public partial class Database
    {
        private readonly CallbackHandler<string> _callbackHandler = new CallbackHandler<string>();
        
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
        public bool AddCallback<T>(string key, Action<T> callback, string name = "", bool unique = false,
            bool removeOnError = false)
        {
            return _callbackHandler.AddCallback(key, callback, name, unique, removeOnError);
        }
        
        /// <summary>
        /// Gets the number of callbacks.
        /// </summary>
        /// <param name="key">Key of callbacks</param>
        /// <param name="name">Required name of callbacks, if any</param>
        /// <returns>Number of callbacks matching criterion</returns>
        public int GetCallbackCount(string key, string name=null)
        {
            return _callbackHandler.GetCallbackCount(key, name);
        }
        
        /// <summary>
        /// Removes callbacks.
        /// </summary>
        /// <param name="key">Key of callbacks</param>
        /// <param name="name">Required name of callbacks, if any</param>
        /// <returns>Number of callbacks removed</returns>
        public int RemoveCallbacks(string key, string name=null)
        {
            return _callbackHandler.RemoveCallbacks(key, name);
        }

        /// <summary>
        /// Invokes callbacks.
        /// </summary>
        /// <param name="key">Key of callbacks</param>
        /// <param name="value">Value of objects to invoke callbacks with</param>
        /// <param name="name">Required name of callbacks to be invoked, if any</param>
        /// <returns>Number of invoked callbacks</returns>
        public int Invoke(string key, object value, string name = null)
        {
            return _callbackHandler.Invoke(key, value, name);
        }
    }
}