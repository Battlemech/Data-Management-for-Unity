using System;
using Data_Management_for_Unity.Runtime.Databases.Structs;
using Data_Management_for_Unity.Runtime.Serializer;
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
    
    public class ValueStorage<T> : ValueStorage
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

        public void BlockingGet(Action<T> safeOperation)
        {
            lock (Id)
            {
                safeOperation.Invoke(Data);
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
    }
}