using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Data_Management_for_Unity.Runtime.Persistence
{
    public static partial class PersistentData2
    {
        public static async Task<List<PersistentObject>> Load(string databaseId)
        {
            //database doesn't exist
            if (!await DoesDatabaseExists(databaseId)) return null;

            return await CreateCommand($"select id, value, type, modCount from '{databaseId}'", async command =>
            {
                //execute lookup
                await using var reader = await command.ExecuteReaderAsync();

                //init return list
                List<PersistentObject> savedObjects = new List<PersistentObject>();

                //read data
                while (await reader.ReadAsync())
                {
                    //deserialize object
                    savedObjects.Add(new PersistentObject(databaseId, reader.GetString(0), reader[1] as byte[],
                        Type.GetType(reader.GetString(2), true), reader.GetInt32(3)));
                }

                return savedObjects;
            });
        }
        
        
    }
}