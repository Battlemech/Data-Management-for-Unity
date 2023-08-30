using System;
using Data_Management_for_Unity.Runtime.Callbacks;

namespace Data_Management_for_Unity.Runtime.Networking.Messaging.Client
{
    public partial class MessageClient
    {
        /// <summary>
        /// Callbacks which will be executed on the main thread.
        /// </summary>
        private readonly CallbackHandler<Type> _mainThreadCallbacks = new CallbackHandler<Type>();

        /// <summary>
        /// Callbacks which will be executed on the receiving thread.
        /// </summary>
        private readonly CallbackHandler<Type> _threadedCallbacks = new CallbackHandler<Type>();

        /// <summary>
        /// Adds a callback, which is invoked whenever an object of the expected type is received
        /// </summary>
        /// <param name="callback">Action invoked when callback is triggered</param>
        /// <param name="name">Name of the callback</param>
        /// <param name="unique">True if callbacks with duplicate names must be prevented</param>
        /// <param name="removeOnError">True if the callbacks must be removed on error</param>
        /// <param name="mainThread">True if the callback will be executed on Unity's main thread, otherwise false</param>
        /// <typeparam name="T">Expected type of object in callback</typeparam>
        /// <returns>True if the callback was added, false if the unique parameter could not be met</returns>
        public bool AddCallback<T>(Action<T> callback, string name = "", bool unique = false, bool removeOnError = false, bool mainThread=false)
        {
            return GetHandler(mainThread).AddCallback(typeof(T), callback, name, unique, removeOnError);
        }

        /// <summary>
        /// Gets the number of callbacks
        /// </summary>
        /// <param name="name">Required name of callbacks, if any</param>
        /// <param name="mainThread">True if the callback was added on the main thread, otherwise false</param>
        /// <returns>Number of callbacks matching criterion</returns>
        public int GetCallbackCount<T>(string name=null, bool mainThread=false)
        {
            return GetHandler(mainThread).GetCallbackCount(typeof(T), name);
        }

        /// <summary>
        /// Removes callbacks
        /// </summary>
        /// <param name="name">Required name of callbacks, if any</param>
        /// <param name="mainThread">True if the callback was added on the main thread, otherwise false</param>
        /// <returns>Number of callbacks removed</returns>
        public int RemoveCallbacks<T>(string name = null, bool mainThread=false)
        {
            return GetHandler(mainThread).RemoveCallbacks(typeof(T), name);
        }

        private CallbackHandler<Type> GetHandler(bool mainThread)
        {
            return mainThread ? _mainThreadCallbacks : _threadedCallbacks;
        }
    }
}