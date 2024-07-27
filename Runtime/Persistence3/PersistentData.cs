using System;
using Mono.Data.Sqlite;

namespace Data_Management_for_Unity.Runtime.Persistence3
{
    public partial class PersistentData : IDisposable
    {
        //connection parameters
        private const string Path = "./Data.sql";
        private const string ConnectionString = "Data Source=" + Path;
        
        //instance management
        private static readonly PersistentData Instance = new PersistentData();
        
        //private attributes
        private static readonly SqliteConnection Connection;
        
        static PersistentData()
        {
            //connect to database
            Connection = new SqliteConnection(ConnectionString);
            Connection.Open();
        }
        
        private static void ExecuteCommand(string command)
        {
            //create command
            using SqliteCommand sqliteCommand = Connection.CreateCommand();
            sqliteCommand.CommandText = command;
            
            //execute command
            sqliteCommand.ExecuteNonQuery();
        }
        
        public void Dispose()
        {
            //close connection on program exit
            Connection.Close();
        }
    }
}