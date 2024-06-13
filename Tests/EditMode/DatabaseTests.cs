using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data_Management_for_Unity.Runtime;
using Data_Management_for_Unity.Runtime.Databases;
using Data_Management_for_Unity.Runtime.Databases.ValueStorages;
using Data_Management_for_Unity.Runtime.Persistence;
using Data_Management_for_Unity.Runtime.Serializer;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Data_Management_for_Unity.Tests.EditMode
{
    public class DatabaseTests
    {
        [UnityTest]
        public IEnumerator TestPersistence()
        {
            return TestPersistenceAsync().AsIEnumerator();
        }

        /// <summary>
        /// Test takes a long time because SQLite backend waits for written data to be confirmed, sends back confirmation.
        /// This process is awaited here and repeated "setCount" times.
        /// </summary>
        private async Task TestPersistenceAsync()
        {
            const string id = nameof(TestPersistence);
            const int setCount = 100;
            
            //clear old data
            PersistentData.DeleteDatabase(id);

            for (int i = 0; i < setCount; i++)
            {
                Database database = new Database(id, true);

                //make sure value was loaded correctly
                Assert.AreEqual(i, database.Get<int>(id).Get(), "Value not loaded");
                
                //update value
                await database.Get<int>(id).Set(i + 1);
                
                //make sure database saved value correctly
                Assert.AreEqual(i + 1, database.Get<int>(id).Get(), "Value not saved in database");

                List<PersistentObject> savedObjects = PersistentData.Load(id);
                Assert.NotNull(savedObjects, "Database not saved");
                Assert.AreEqual(1, savedObjects.Count, "Wrong amount of values saved");
                Assert.AreEqual(i + 1, SerializationPCK.Deserialize<int>(savedObjects[0].Value), "Wrong value saved");
            }
        }

        [Test]
        public void TestListPersistence()
        {
            const string id = nameof(TestListPersistence);
            const int addCount = 100;
            
            //clear old data
            PersistentData.DeleteDatabase(id);
            
            //create database
            Database database = new Database(id, true);

            for (int i = 0; i < addCount; i++)
            {
                //make sure value was loaded correctly
                for (int j = 0; j < i; j++)
                {
                    Assert.Contains(j, database.Get<List<int>>(id).Get(), "Value not loaded");
                }
                
                //add i to list
                database.Get<List<int>>(id).Add(i);
            }
        }
    }
}