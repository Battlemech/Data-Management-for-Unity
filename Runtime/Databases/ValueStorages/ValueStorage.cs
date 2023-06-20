using System;
using Data_Management_for_Unity.Runtime.Serializer;
using UnityEditor.VersionControl;
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

        public abstract void UnsafeSet(object value);
    }
    
    public partial class ValueStorage<T> : ValueStorage
    {
        private T _data;
        
        public ValueStorage(string id, Database database) : base(id, database)
        {
            
        }

        public T Get()
        {
            lock (Id)
            {
                return _data;
            }
        }

        public void BlockingGet(Action<T> safeOperation)
        {
            lock (Id)
            {
                safeOperation.Invoke(_data);
            }
        }

        public Task Set(T data)
        {
            byte[] value;
            Type type;
            
            lock (Id)
            {
                //update value
                _data = data;
                
                //save its serialized version
                value = Serialization.Serialize(data, out type);
            }

            //delegate internal logic to background to increase performance
            return Database.OnSet(Id, value, type);
        }
        
        public Task BlockingSet(ModifyDelegate<T> modifyDelegate)
        {
            byte[] value;
            Type type;
            
            lock (Id)
            {
                //update value
                _data = modifyDelegate.Invoke(_data);
                
                //save its serialized version
                value = Serialization.Serialize(_data, out type);
            }
            
            //delegate internal logic to background to increase performance
            return Task.Run((() => {Database.OnSet(Id, value, type); }));
        }

        public override void UnsafeSet(object value)
        {
            switch (value)
            {
                case T data:
                    Set(data);
                    break;
                case null:
                    Set(default);
                    break;
                default:
                    throw new ArgumentException($"Expected type {typeof(T)}, but got {value?.GetType()}");
            }
        }
    }
}