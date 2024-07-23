using System.Collections.Generic;
using Data_Management_for_Unity.Runtime.Networking.Synchronising.Client;
using Data_Management_for_Unity.Runtime.Objects;

namespace Data_Management_for_Unity.Runtime.Databases
{
    public class DatabaseManager
    {
        /// <summary>
        /// Default working instance of the database manager
        /// </summary>
        public static readonly DatabaseManager Instance = new DatabaseManager();
        
        private readonly Dictionary<string, Database> _databases = new Dictionary<string, Database>();

        
        public Database GetDatabase(string id, bool? isSynchronised = null, bool? isPersistent = null)
        {
            lock (_databases)
            {
                //database exists locally
                if (_databases.TryGetValue(id, out Database database)) return database;

                //create database
                database = new Database(id, 
                    isPersistent: isPersistent ?? false,
                    isSynchronised: isSynchronised ?? false);
                
                //track its instance locally
                _databases.Add(id, database);

                return database;
            }
        }

        public Database GetDatabase(DatabaseReference reference)
        {
            return GetDatabase(reference.Id, reference.IsSynchronised, reference.IsPersistent);
        }
    }
}