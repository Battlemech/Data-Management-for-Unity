using System;
using Data_Management_for_Unity.Runtime.Databases.Structs;
using Data_Management_for_Unity.Runtime.Serializer;
using Data_Management_for_Unity.Runtime.Threading;
using UnityEditor.VersionControl;
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

        public Task Set(T data)
        {
            byte[] value;
            Type type;
            
            lock (Id)
            {
                //update value
                Data = data;
                
                //save its serialized version
                value = Serialization.Serialize(data, out type);
            }

            //delegate internal logic to background to increase performance
            return Database.OnSet(Id, value, type);
        }
        
        public Task Modify(ModifyDelegate<T> modifyDelegate)
        {
            byte[] value;
            Type type;
            
            lock (Id)
            {
                //update value
                Data = modifyDelegate.Invoke(Data);
                
                //save its serialized version
                value = Serialization.Serialize(Data, out type);
            }
            
            //delegate internal logic to background to increase performance
            return Database.OnModify(Id, value, type, modifyDelegate);
        }

        /// <summary>
        /// Invokes an action exactly one time as soon as the value isn't null or default 
        /// </summary>
        /// <param name="onInitialized">Action to perform once value was initialized</param>
        /// <param name="mainThread">True if the action is supposed to be delegated to unity's main thread, otherwise false</param>
        public void OnInitialized(Action<T> onInitialized, bool mainThread=false) => Database.OnInitialized(Id, onInitialized, mainThread);
        
        protected internal override void InternalSet(byte[] bytes, Type type)
        {
            object value = Serialization.Deserialize(bytes, type);
            
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
                return Serialization.Serialize(Data, out type);
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