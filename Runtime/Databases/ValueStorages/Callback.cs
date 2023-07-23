using System;
using Data_Management_for_Unity.Runtime.Networking.Messaging;
using Data_Management_for_Unity.Runtime.Threading;

namespace Data_Management_for_Unity.Runtime.Databases.ValueStorages
{
    public partial class ValueStorage<T>
    {
        /// <summary>
        /// Adds a callback.
        /// </summary>
        /// <param name="callback">Action invoked when callback is triggered</param>
        /// <param name="name">Name of the callback</param>
        /// <param name="unique">True if callbacks with duplicate names must be prevented</param>
        /// <param name="removeOnError">True if the callbacks must be removed on error</param>
        /// <param name="invoke">True if the callback is invoked, otherwise false</param>
        /// <param name="mainThread">True if the callback will be executed on Unity's main thread, otherwise false</param>
        /// <returns>True if the callback was added, false if the unique parameter could not be met</returns>
        public bool AddCallback(Action<T> callback, string name = "", bool unique = false,
            bool removeOnError = false, bool invoke = false, bool mainThread = Options.MainThreadDefault)
        {
            return Database.AddCallback(Id, callback, name, unique, removeOnError, invoke, mainThread);
        }
        
        /// <summary>
        /// Gets the number of callbacks.
        /// </summary>
        /// <param name="name">Required name of callbacks, if any</param>
        /// <param name="mainThread">True if the callback was added on the main thread, otherwise false</param>
        /// <returns>Number of callbacks matching criterion</returns>
        public int GetCallbackCount(string name=null, bool mainThread=false)
        {
            return Database.GetCallbackCount(Id, name);
        }
        
        /// <summary>
        /// Removes callbacks.
        /// </summary>
        /// <param name="name">Required name of callbacks, if any</param>
        /// <param name="mainThread">True if the callback was added on the main thread, otherwise false</param>
        /// <returns>Number of callbacks removed</returns>
        public int RemoveCallbacks(string name=null, bool mainThread=false)
        {
            return Database.RemoveCallbacks(Id, name, mainThread);
        }
        
        /// <summary>
        /// Invokes threaded and mainThread callbacks.
        /// </summary>
        /// <param name="value">Value of objects to invoke callbacks with</param>
        /// <param name="name">Required name of callbacks to be invoked, if any</param>
        public void Invoke(object value, string name = null)
        {
            Database.Invoke(Id, value, name);
        }
    }
}