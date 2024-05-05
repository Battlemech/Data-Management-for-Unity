using Mono.Data.Sqlite;

namespace Data_Management_for_Unity.Runtime.Persistence
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
    }
}