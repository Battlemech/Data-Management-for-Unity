using System;
using System.Collections.Generic;
using System.Threading;
using Data_Management_for_Unity.Runtime.Databases.ValueStorages;
using Data_Management_for_Unity.Runtime.Networking.Messaging;

namespace Data_Management_for_Unity.Runtime.Databases
{
    public partial class Database
    {
        private int _onInitializedTracker;

        /// <summary>
        /// Invokes an action exactly one time as soon as the value isn't null or default 
        /// </summary>
        /// <param name="id">Id of the value</param>
        /// <param name="onInitialized">Action to perform once value was initialized</param>
        /// <param name="mainThread">True if the action is supposed to be executed to Unity's main thread, otherwise false</param>
        /// <typeparam name="T">Type of value</typeparam>
        public void OnInitialized<T>(string id, Action<T> onInitialized, bool mainThread=Options.MainThreadDefault)
        {
            ValueStorage<T> valueStorage = Get<T>(id);
            
            valueStorage.BlockingGet((value) =>
            {
                if (TryInvoke(value, onInitialized)) return;

                //get thread safe index increment
                string callbackName = $"SYSTEM/INTERNAL/{id}-{Interlocked.Increment(ref _onInitializedTracker)}";
                
                //invoke action once if value is not null or default
                valueStorage.AddCallback((obj =>
                {
                    //remove callback if invocation was successful
                    if (TryInvoke(obj, onInitialized)) valueStorage.RemoveCallbacks(callbackName);
                }), callbackName, mainThread:mainThread); 
            }, mainThread);
        }

        private static bool TryInvoke<T>(T obj, Action<T> onInitialized)
        {
            if (IsNullOrDefault(obj)) return false;
            
            onInitialized.Invoke(obj);

            return true;
        }

        private static bool IsNullOrDefault<T>(T obj)
        {
            return EqualityComparer<T>.Default.Equals(obj, default(T));
        }
    }
}