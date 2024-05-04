using System;
using System.Threading.Tasks;
using Mono.Data.Sqlite;

namespace Data_Management_for_Unity.Runtime.Persistence
{
    public static partial class PersistentData2
    {
        public static Task CreateDatabase(string databaseId)
        {
            return CreateCommand($"create table if not exists '{databaseId}'(id MESSAGE_TEXT PRIMARY KEY, value BLOB, type MESSAGE_TEXT, modCount INTEGER)", true);
        }
        
        public static Task<bool> DoesDatabaseExists(string databaseId)
        {
            return CreateCommand($"SELECT * FROM sqlite_master WHERE type='table' AND name='{databaseId}'",
                async command =>
            {
                //execute lookup
                await using var reader = await command.ExecuteReaderAsync(); 
                
                //if reader can read at least one column, table exists
                return await reader.ReadAsync();
            }, true);
        }
        
        public static Task DeleteDatabase(string databaseId)
        {
            return CreateCommand($"drop table if exists '{databaseId}'", true);
        }
        
        private static async Task CreateCommand(string commandString, bool execute = false)
        {
            await CreateCommand(commandString,  async _ => Task.CompletedTask, execute);
        }
        
        private static async Task<T> CreateCommand<T>(string commandString, Func<SqliteCommand, Task<T>> onCommand, bool execute = false)
        {
            return await DelegateOperation(async connection =>
            {
                //setup command
                await using SqliteCommand command = connection.CreateCommand();
                command.CommandText = commandString;
                
                //execute command
                if(execute) await command.ExecuteNonQueryAsync();

                //invoke callback
                return await onCommand.Invoke(command);
            });
        }
    }
}