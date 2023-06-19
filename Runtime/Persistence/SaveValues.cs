using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Mono.Data.Sqlite;
using UnityEngine;

namespace Data_Management_for_Unity.Runtime.Persistence
{
    public static partial class PersistentData
    {
        /// <summary>
        /// True if a thread is currently saving data, otherwise false
        /// </summary>
        public static Task SavingData { get; private set; }
        
        /// <summary>
        /// Returns the amount of values which still have to be set.
        /// </summary>
        public static int EnqueuedData => ToSave.Count;
        
        /// <summary>
        /// Queue of values which have to be saved persistently.
        /// </summary>
        private static readonly ConcurrentQueue<SerializedObject> ToSave = new ConcurrentQueue<SerializedObject>();

        public static Task Save(string databaseId, string valueId, byte[] value, Type type, int modCount)
        {
            ToSave.Enqueue(new SerializedObject(databaseId, valueId, value, type, modCount));
            
            lock (ToSave)
            {
                //data is already being saved by another task
                return SavingData ??
                       //delegate saving of persistant data to a task
                       Task.Run(SaveQueuedData);
            }
        }

        private static void SaveQueuedData()
        {
            while (true)
            {
                //establish connection to database
                using SqliteConnection connection = new SqliteConnection(ConnectionString);
                connection.Open();
                
                //save all queued data
                while (ToSave.TryDequeue(out SerializedObject r))
                {
                    //setup set
                    using SqliteCommand command = connection.CreateCommand();
                    command.CommandText = $"insert or replace into '{r.DatabaseId}'(id, value, type, modCount) values ('{r.ValueId}', :value, :type, :modCount)";
                    command.Parameters.AddWithValue(":value", r.Value);
                    command.Parameters.AddWithValue(":type", r.Type.AssemblyQualifiedName);
                    command.Parameters.AddWithValue(":modCount", r.ModCount);
                    
                    //commit
                    command.ExecuteNonQuery();
                }

                lock (ToSave)
                {
                    //continue processing data, if necessary
                    if(!ToSave.IsEmpty) continue;
                    
                    //all data was processed
                    SavingData = null;
                    return;
                }
            }
        }
    }
}