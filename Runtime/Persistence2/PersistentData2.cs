using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mono.Data.Sqlite;
using UnityEngine;

namespace Data_Management_for_Unity.Runtime.Persistence
{
    public static partial class PersistentData2
    {
        private const string Path = "./Data.sql";
        private const string ConnectionString = "Data Source=" + Path;
        
        /// <summary>
        /// Task currently processing database operations
        /// </summary>
        public static Task DatabaseProcessingTask { get; private set; }
        
        /// <summary>
        /// Queue of database operations to be executed
        /// </summary>
        private static readonly ConcurrentQueue<Func<SqliteConnection, Task>> TaskQueue = new ConcurrentQueue<Func<SqliteConnection, Task>>();
        
        private static Task<T> DelegateOperation<T>(Func<SqliteConnection, Task<T>> operation)
        {
            var tcs = new TaskCompletionSource<T>();
            
            //write task result to created task
            TaskQueue.Enqueue(async connection =>
            {
                try
                {
                    tcs.SetResult(await operation.Invoke(connection));
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
            });

            //ensure only one thread is entering critical area at the same time
            lock (TaskQueue)
            {
                //start processing tasks if not already running
                DatabaseProcessingTask ??= StartProcessing();
            }
            
            return tcs.Task;
        }
        
        private static async Task StartProcessing()
        {
            //establish connection
            await using SqliteConnection connection = new SqliteConnection(ConnectionString);

            try
            {
                //process all commands
                await Process(connection);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                connection.Close();
            }
        }

        private static async Task Process(SqliteConnection connection)
        {
            //open connection
            await connection.OpenAsync();
                
            //start transaction
            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                while (true)
                {
                    //process tasks
                    while (TaskQueue.TryDequeue(out var task))
                    {
                        await task.Invoke(connection);
                    }

                    //stop processing tasks if queue is empty
                    lock (TaskQueue)
                    {
                        //more tasks were added while processing
                        if (!TaskQueue.IsEmpty) continue;
                            
                        //commit transaction on successful processing
                        transaction.Commit();

                        DatabaseProcessingTask = null;
                        return;
                    }
                }
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}