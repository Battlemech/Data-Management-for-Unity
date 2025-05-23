using System.Collections.Generic;
using System.Linq;
using Mono.Data.Sqlite;

namespace Data_Management_for_Unity.Runtime.Persistence3
{
    public partial class PersistentData
    {
        public static void CreateDatabase(string databaseId)
        {
            ExecuteCommand($"create table if not exists '{databaseId}'(id MESSAGE_TEXT PRIMARY KEY, value BLOB, type MESSAGE_TEXT, modCount INTEGER)");
        }
        
        public static bool DoesDatabaseExists(string databaseId)
        {
            //create command
            using SqliteCommand sqliteCommand = Connection.CreateCommand();
            sqliteCommand.CommandText = $"SELECT * FROM sqlite_master WHERE type='table' AND name='{databaseId}'";
            
            //execute command
            using SqliteDataReader reader = sqliteCommand.ExecuteReader();
            
            //if reader can read at least one column, table exists
            return reader.Read();
        }
        
        public static void DeleteDatabase(string databaseId)
        {
            ExecuteCommand($"drop table if exists '{databaseId}'");
        }
        
        public static void DeleteAllDatabases()
        {
            // Retrieve all table names from the sqlite_master table.
            List<string> tableNames = new List<string>();
            using (SqliteCommand command = Connection.CreateCommand())
            {
                command.CommandText = "SELECT name FROM sqlite_master WHERE type='table'";
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // Collect the table name.
                        tableNames.Add(reader.GetString(0));
                    }
                }
            }
            
            if(tableNames.Count == 0) return;
    
            // Drop each table in a single command to increase performance.
            ExecuteCommand(tableNames.Select((name => $"DROP TABLE IF EXISTS '{name}'; ")).Aggregate((a, b) => a + b));
        }
    }
}