using System;
using System.Collections.Generic;
using Mono.Data.Sqlite;
using UnityEngine;

namespace Data_Management_for_Unity.Runtime.Persistence
{
    public static partial class PersistentData
    {
        public static bool TryLoadDatabase(string databaseId, out List<SerializedObject> savedObjects)
        {
            //init return list
            savedObjects = new List<SerializedObject>();
            
            //establish connection
            using SqliteConnection connection = new SqliteConnection(ConnectionString);
            connection.Open();
            
            //create read command
            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = $"select id, value, type, modCount from '{databaseId}'";

            //try reading data
            try
            {
                using SqliteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    //deserialize object
                    savedObjects.Add(new SerializedObject(databaseId, reader.GetString(0), reader[1] as byte[], Type.GetType(reader.GetString(2), true), reader.GetInt32(3)));
                }
            }
            catch (SqliteException e)
            {
                //table didn't exist
                if (e.Message.Contains($"no such table: {databaseId}")) return false;
                
                //other exception
                throw;
            }
            
            //data was retrieved successfully
            return true;
        }
    }
}