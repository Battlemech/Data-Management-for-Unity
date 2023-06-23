using System;
using System.Threading.Tasks;
using Mono.Data.Sqlite;

namespace Data_Management_for_Unity.Runtime.Persistence
{
    public static partial class PersistentData
    {
        private const string Path = "./Data.sql";
        private const string ConnectionString = "Data Source=" + Path;

        static PersistentData()
        {
            //make sure local database exists
            SqliteConnection.CreateFile(Path);
        }

        public static void CreateDatabase(string databaseId)
        {
            ExecuteCommand($"create table if not exists '{databaseId}'(id MESSAGE_TEXT PRIMARY KEY, value BLOB, type MESSAGE_TEXT, modCount INTEGER)");
        }

        public static bool DoesDatabaseExists(string databaseId)
        {
            //establish connection
            using SqliteConnection connection = new SqliteConnection(ConnectionString);
            connection.Open();

            //setup command
            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = $"SELECT * FROM sqlite_master WHERE type='table' AND name='{databaseId}'";

            //execute lookup
            using SqliteDataReader reader = command.ExecuteReader();
            //if reader can read at least one column, table exists
            return reader.Read();
        }

        public static void DeleteDatabase(string databaseId)
        {
            ExecuteCommand($"drop table if exists '{databaseId}'");
        }

        private static void ExecuteCommand(string commandString)
        {
            //establish connection
            using SqliteConnection connection = new SqliteConnection(ConnectionString);
            connection.Open();

            //setup command
            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = commandString;
            
            //execute command
            command.ExecuteNonQuery();
            connection.Close();
        }
    }
}