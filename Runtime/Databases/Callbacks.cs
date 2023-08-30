using System;
using Data_Management_for_Unity.Runtime.Callbacks;
using Data_Management_for_Unity.Runtime.Networking.Messaging;
using Data_Management_for_Unity.Runtime.Threading;

namespace Data_Management_for_Unity.Runtime.Databases
{
    public partial class Database
    {
        /// <summary>
        /// Callbacks which will be executed on the main thread.
        /// </summary>
        private readonly CallbackHandler<string> _mainThreadCallbacks = new CallbackHandler<string>();

        /// <summary>
        /// Callbacks which will be executed on the receiving thread.
        /// </summary>
        private readonly CallbackHandler<string> _threadedCallbacks = new CallbackHandler<string>();

        /// <summary>
        /// Adds a callback.
        /// </summary>
        /// <param name="key">Key of the callback function</param>
        /// <param name="callback">Action invoked when callback is triggered</param>
        /// <param name="name">Name of the callback</param>
        /// <param name="unique">True if callbacks with duplicate names must be prevented</param>
        /// <param name="removeOnError">True if the callbacks must be removed on error</param>
        /// <param name="invoke">True if the callback is invoked, otherwise false</param>
        /// <param name="type">Defines on which thread the callback will be executed</param>
        /// <param name="mainThread">True if the callback will be executed on Unity's main thread, otherwise false</param>
        /// <typeparam name="T">Expected type of object in callback</typeparam>
        /// <returns>True if the callback was added, false if the unique parameter could not be met</returns>
        public bool AddCallback<T>(string key, Action<T> callback, string name = "", bool unique = false,
            bool removeOnError = false, bool invoke=false, bool mainThread = false)
        {
            //Add callback to specified thread handler
            bool added = GetHandler(mainThread).AddCallback(key, callback, name, unique, removeOnError);

            //unique parameter of added callback couldn't be met
            if (!added) return false;
            
            //invoke callback if necessary
            if (invoke)
            {
                if (mainThread) MainThreadRunner.Delegate((() => Get<T>(key).BlockingGet(callback.Invoke)));
                else Get<T>(key).BlockingGet(callback.Invoke);
            }

            return true;
        }
        
        /// <summary>
        /// Gets the number of callbacks.
        /// </summary>
        /// <param name="key">Key of callbacks</param>
        /// <param name="name">Required name of callbacks, if any</param>
        /// <param name="mainThread">True if the callback was added to Unity's main thread, otherwise false</param>
        /// <returns>Number of callbacks matching criterion</returns>
        public int GetCallbackCount(string key, string name=null, bool mainThread=false)
        {
            return GetHandler(mainThread).GetCallbackCount(key, name);
        }
        
        /// <summary>
        /// Removes callbacks.
        /// </summary>
        /// <param name="key">Key of callbacks</param>
        /// <param name="name">Required name of callbacks, if any</param>
        /// <param name="mainThread">True if the callback was added on the main thread, otherwise false</param>
        /// <returns>Number of callbacks removed</returns>
        public int RemoveCallbacks(string key, string name=null, bool mainThread=false)
        {
            return GetHandler(mainThread).RemoveCallbacks(key, name);
        }

        /// <summary>
        /// Invokes threaded and mainThread callbacks.
        /// </summary>
        /// <param name="key">Key of callbacks</param>
        /// <param name="value">Value of objects to invoke callbacks with</param>
        /// <param name="name">Required name of callbacks to be invoked, if any</param>
        /// <returns>Number of invoked threaded callbacks</returns>
        public void Invoke(string key, object value, string name = null)
        {
            //invoke callbacks on a thread
            _threadedCallbacks.Invoke(key, value, name);

            //don't notify main thread of no callbacks for key and name exist
            if (_mainThreadCallbacks.GetCallbackCount(key, name) == 0) return;

            //delegate invocation of callbacks to Unity's main thread
            MainThreadRunner.Delegate((() => _mainThreadCallbacks.Invoke(key, value, name)));
        }
        
        private CallbackHandler<string> GetHandler(bool mainThread)
        {
            return mainThread ? _mainThreadCallbacks : _threadedCallbacks;
        }
    }
}