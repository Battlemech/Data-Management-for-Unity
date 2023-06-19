using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Data_Management_for_Unity.Runtime;
using Data_Management_for_Unity.Runtime.Persistence;
using NUnit.Framework;
using UnityEngine.TestTools;
using Debug = UnityEngine.Debug;

namespace Data_Management_for_Unity.Tests.EditMode
{
    public class SqliteTests
    {
        [Test]
        public void TableCreation()
        {
            const string id = "SimpleTestTable";
            
            //make sure table doesn't exist
            Assert.IsFalse(PersistentData.DoesDatabaseExists(id));
            
            //create table
            PersistentData.CreateDatabase(id);
            Assert.IsTrue(PersistentData.DoesDatabaseExists(id));
            
            //delete table
            PersistentData.DeleteDatabase(id);
            Assert.IsFalse(PersistentData.DoesDatabaseExists(id));
        }

        [UnityTest]
        public IEnumerator SaveData()
        {
            return SaveDataAsync().AsIEnumerator();
        }

        private async Task SaveDataAsync()
        {
            //how many values are saved
            const int saveCount = 1000;
            
            //values to save
            string id = nameof(SaveData);
            byte[] value = new byte[1] { 3 };
            Type type = typeof(string);
            int modCount = 123;
            
            //clear old data
            PersistentData.DeleteDatabase(id);
            
            //create table
            PersistentData.CreateDatabase(id);
            
            //wait for data to be saved
            Stopwatch stopwatch = Stopwatch.StartNew();
            
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < saveCount; i++)
            {
                tasks.Add(PersistentData.Save(id, i.ToString(), value, type, modCount));    
            }
            
            Debug.Log($"Delegated persistently saving {saveCount} values after {stopwatch.ElapsedMilliseconds} ms");
            await Task.WhenAll(tasks);
            Debug.Log($"Delegated tasks saved {saveCount} values after {stopwatch.ElapsedMilliseconds} ms");

            Assert.AreEqual(0, PersistentData.EnqueuedData);
            Assert.IsNull(PersistentData.SavingData);
            
            //make sure they were saved
            stopwatch.Restart();
            Assert.IsTrue(PersistentData.TryLoadDatabase(id, out List<SerializedObject> savedObjects));
            Debug.Log($"Loading {saveCount} values took: {stopwatch.ElapsedMilliseconds} ms");
            Assert.AreEqual(saveCount, savedObjects.Count);

            //make sure data was saved correctly
            foreach (var so in savedObjects)
            {
                Assert.AreEqual(id, so.DatabaseId);
                Assert.AreEqual(value, so.Value);
                Assert.AreEqual(type, so.Type);
                Assert.AreEqual(modCount, so.ModCount);
            }

            //make sure each object id was saved correctly
            for (int i = 0; i < saveCount; i++)
            {
                Assert.AreEqual(1, savedObjects.Where((o => o.ValueId == i.ToString())).Count());
            }
            
        }
    }
}