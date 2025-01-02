using System;
using System.Threading;
using System.Threading.Tasks;
using Data_Management_for_Unity.Runtime.Databases.Structs;
using Data_Management_for_Unity.Runtime.Serializer;
using Data_Management_for_Unity.Runtime.Threading;
using UnityEngine;
using Task = System.Threading.Tasks.Task;

namespace Data_Management_for_Unity.Runtime.Databases.ValueStorages
{
    public abstract class ValueStorage
    {
        public readonly string Id;
        public readonly Database Database;

        protected ValueStorage(string id, Database database)
        {
            Id = id;
            Database = database;
        }
        
        /// <summary>
        /// Updates the local value as a result of an internal operation
        /// (Synchronisation or Persistence). Does not invoke callbacks, synchronisation or persistence.
        /// </summary>
        protected internal abstract void InternalSet(byte[] bytes, Type type);

        protected internal abstract byte[] Serialize(out Type type);
    }
    
    public partial class ValueStorage<T> : ValueStorage
    {
        protected internal T Data;
        
        public ValueStorage(string id, Database database) : base(id, database)
        {
            
        }

        public T Get()
        {
            lock (Id)
            {
                return Data;
            }
        }

        /// <summary>
        /// Blocks any changes to this value while an operation is performed
        /// </summary>
        /// <param name="safeOperation">Operation to perform</param>
        /// <param name="mainThread">True if the execution is supposed to be delegated to Unity's main thread, otherwise false</param>
        public void BlockingGet(Action<T> safeOperation, bool mainThread=false)
        {
            //delegate operation to main thread if necessary
            if (mainThread)
            {
                //prevent infinite loops
                MainThreadRunner.Delegate((() => BlockingGet(safeOperation, false)));
                return;
            }
            
            lock (Id)
            {
                safeOperation.Invoke(Data);
            }
        }

        public TOut BlockingGet<TOut>(SafeOperation<T, TOut> safeOperation)
        {
            lock (Id)
            {
                return safeOperation.Invoke(Data);
            }
        }

        /// <summary>
        /// Overwrites the current value and synchronises the result in the network
        /// </summary>
        /// <param name="data">New value</param>
        /// <param name="onConfirmed">Action executed once the new value was confirmed remotely. Executed on a thread</param>
        /// <param name="mainThread">True if the onConfirmed action is executed on Unity's main thread</param>
        /// <param name="logException">True if the async task should be checked for exceptions, otherwise false</param>
        /// <returns>Internal task synchronising values and saving data persistently</returns>
        public Task Set(T data, Action<T> onConfirmed = null, bool mainThread=false, bool logException=true)
        {
            byte[] value;
            Type type;
            
            lock (Id)
            {
                //update value
                Data = data;
                
                //save its serialized version
                value = SerializationPCK.Serialize(data, out type);
            }
            
            //delegate action on main thread, if necessary
            var action = mainThread && onConfirmed != null ? (obj) => MainThreadRunner.Delegate(() => onConfirmed.Invoke(obj)) : onConfirmed;

            //delegate internal logic to background to increase performance
            var task = Database.OnSet(Id, value, type, action);

            //log exception of async task, if desired
            return logException ? task.LogOnFailure() : task;
        }

        /// <summary>
        /// Modifies the current value and synchronises the result in the network.
        /// </summary>
        /// <param name="modifyDelegate">Operation to perform on up-to-date data</param>
        /// <param name="safe">
        /// A safe operation requests up-to-date data from server and performs the operation after.
        /// An unsafe operation assumes up-to-date data exists locally and instantly performs the operation, executing the operation a second time if the local data wasn't synchronised with the server.
        /// For performance, usage of safe operations is discouraged unless temporary inconsistent states want to be avoided or delegates need to be executed exactly once.
        /// </param>
        /// <param name="onConfirmed">Action executed once the new value was confirmed remotely. Executed on a thread</param>
        /// <param name="mainThread">True if the onConfirmed action is executed on Unity's main thread</param>
        /// <param name="logException">True if the async task should be checked for exceptions, otherwise false</param>
        /// <returns>Internal task synchronising values and saving data persistently</returns>
        public Task Modify(ModifyDelegate<T> modifyDelegate, bool safe=false, Action<T> onConfirmed = null, bool mainThread=false, bool logException=true)
        {
            byte[] value;
            Type type;

            lock (Id)
            {
                //only invoke operation if inconsistent states are allowed
                if (!safe) Data = modifyDelegate.Invoke(Data);

                //save its serialized version
                value = SerializationPCK.Serialize(Data, out type);
            }

            //delegate action on main thread, if necessary
            var action = mainThread && onConfirmed != null ? (obj) => MainThreadRunner.Delegate(() => onConfirmed.Invoke(obj)) : onConfirmed;
            
            //delegate internal logic to background to increase performance
            var task = Database.OnModify(Id, value, type, modifyDelegate, safe, action);
            
            //log exception of async task, if desired
            return logException ? task.LogOnFailure() : task;
        }

        /// <summary>
        /// Modifies the current value and synchronises the result in the network.
        /// </summary>
        /// <param name="modifyDelegate">Operation to perform on up-to-date data</param>
        /// <param name="safe">
        /// A safe operation requests up-to-date data from server and performs the operation after.
        /// An unsafe operation assumes up-to-date data exists locally and instantly performs the operation, executing the operation a second time if the local data wasn't synchronised with the server.
        /// For performance, usage of safe operations is discouraged unless temporary inconsistent states want to be avoided or delegates need to be executed exactly once.
        /// </param>
        /// <returns>Internal task synchronising values and saving data persistently</returns>
        public Task<T> ModifyAsync(ModifyDelegate<T> modifyDelegate, bool safe=false)
        {
            var tcs = new TaskCompletionSource<T>();

            // modify value
            Modify(modifyDelegate, safe, r =>
            {
                // set the result
                tcs.SetResult(r);
            }, false);

            // return the task
            return tcs.Task;
        }

        /// <summary>
        /// Invokes an action exactly one time as soon as the value isn't null or default 
        /// </summary>
        /// <param name="onInitialized">Action to perform once value was initialized</param>
        /// <param name="mainThread">True if the action is supposed to be delegated to unity's main thread, otherwise false</param>
        public void OnInitialized(Action<T> onInitialized, bool mainThread=true) => Database.OnInitialized(Id, onInitialized, mainThread);

        public void OnInitialized(Func<T, Task> onInitialized, bool mainThread=true)
            => OnInitialized(onInitialized.AsAction(), mainThread);
        
        public Task<T> OnInitialized(bool mainThread=true) => Database.OnInitialized<T>(Id, mainThread);
        
        protected internal override void InternalSet(byte[] bytes, Type type)
        {
            object value = SerializationPCK.Deserialize(bytes, type);
            
            switch (value)
            {
                case T data:
                    InternalSet(data);
                    return;
                case null:
                    InternalSet(default);
                    return;
                default:
                    throw new ArgumentException($"Expected type {typeof(T)}, but got {value?.GetType()}");
            }
        }

        protected internal override byte[] Serialize(out Type type)
        {
            lock (Id)
            {
                return SerializationPCK.Serialize(Data, out type);
            }
        }

        private void InternalSet(T data)
        {
            lock (Id)
            {
                Data = data;
            }
        }
        
        public static implicit operator T(ValueStorage<T> valueStorage) => valueStorage.Get();
    }
}