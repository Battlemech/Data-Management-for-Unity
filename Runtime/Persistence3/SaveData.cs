using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Mono.Data.Sqlite;

namespace Data_Management_for_Unity.Runtime.Persistence3
{
    public partial class PersistentData
    {
        public static Task TransactionTask { get; private set; }
        
        /// <summary>
        /// Database modification operations, bundled into a transaction.
        /// </summary>
        private static readonly ConcurrentQueue<SqliteCommand> transactionQueue = new ConcurrentQueue<SqliteCommand>();
        
        public static Task Save(string databaseId, string valueId, byte[] value, Type type, int modCount)
        {
            //create set command
            string commandText = $"insert or replace into '{databaseId}'(id, value, type, modCount) " +
                                 $"values('{valueId}', @value, '{type.AssemblyQualifiedName}', {modCount})";
            
            SqliteCommand command = Connection.CreateCommand();
            command.CommandText = commandText;
            
            //add value
            command.Parameters.AddWithValue("@value", value);
            
            //schedule command execution
            transactionQueue.Enqueue(command);

            lock (transactionQueue)
            {
                //start transaction task, if necessary
                return TransactionTask ??= Task.Run(ExecuteTransaction);
            }
        }
        
        private static void ExecuteTransaction()
        {
            while (true)
            {
                //create transaction
                using SqliteTransaction transaction = Connection.BeginTransaction();
            
                //execute all commands
                while (transactionQueue.TryDequeue(out SqliteCommand command))
                {
                    //execute command
                    command.Transaction = transaction;
                    command.ExecuteNonQuery();
                    
                    //dispose command
                    command.Dispose();
                }
            
                //commit transaction
                transaction.Commit();

                lock (transactionQueue)
                {
                    //more commands to execute
                    if(!transactionQueue.IsEmpty) continue;
                    
                    //stop processing if all commands have been executed
                    TransactionTask = null;
                    return;
                }
            }
        }
    }
}