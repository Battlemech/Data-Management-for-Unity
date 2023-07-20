using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Data_Management_for_Unity.Runtime.Serializer;
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
        private static readonly ConcurrentQueue<PersistentObject> ToSave = new ConcurrentQueue<PersistentObject>();

        public static Task Save(string databaseId, string valueId, byte[] value, Type type, int modCount)
        {
            ToSave.Enqueue(new PersistentObject(databaseId, valueId, value, type, modCount));
            
            //todo: Implement this task queue for all SQLite commands
            lock (ToSave)
            {
                //delegate saving of persistant data to thread
                return SavingData ??= Task.Run(SaveQueuedData);
            }
        }

        private static void SaveQueuedData()
        {
            while (true)
            {
                //establish connection to database
                using SqliteConnection connection = new SqliteConnection(ConnectionString);
                connection.Open();

                //use a transaction to massively speed up the speed of each set: Data is written all at once, not one after another
                using SqliteTransaction transaction = connection.BeginTransaction();
                
                //save all queued data
                while (ToSave.TryDequeue(out PersistentObject r))
                {
                    //setup set
                    using SqliteCommand command = connection.CreateCommand();
                    command.CommandText = $"insert or replace into '{r.DatabaseId}'(id, value, type, modCount) values ('{r.ValueId}', :value, :type, :modCount)";
                    command.Parameters.AddWithValue(":value", r.Value);
                    command.Parameters.AddWithValue(":type", r.Type.AssemblyQualifiedName);
                    command.Parameters.AddWithValue(":modCount", r.ModCount);
                    
                    //queue changes
                    command.ExecuteNonQuery();
                }
                
                //commit queued changes
                transaction.Commit();

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