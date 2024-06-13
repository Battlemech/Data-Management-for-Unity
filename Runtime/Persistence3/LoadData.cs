using System.Collections.Generic;

namespace Data_Management_for_Unity.Runtime.Persistence
{
    public partial class PersistentData
    {
        public static List<PersistentObject> Load(string databaseId)
        {
            //no data to load
            if(!DoesDatabaseExists(databaseId)) return null;
            
            //create command
            using var command = Connection.CreateCommand();
            command.CommandText = $"select id, value, type, modCount from '{databaseId}'";
            
            //execute lookup
            using var reader = command.ExecuteReader();
            
            //init return list
            List<PersistentObject> savedObjects = new List<PersistentObject>();
            
            //read data
            while (reader.Read())
            {
                //deserialize object
                savedObjects.Add(new PersistentObject(databaseId, reader.GetString(0), reader[1] as byte[],
                    System.Type.GetType(reader.GetString(2), true), reader.GetInt32(3)));
            }
            
            return savedObjects;
        }
    }
}