using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data_Management_for_Unity.Runtime;
using Data_Management_for_Unity.Runtime.Databases;
using Data_Management_for_Unity.Runtime.Networking.Synchronising.Client;
using Data_Management_for_Unity.Runtime.Persistence;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Data_Management_for_Unity.Tests.PlayMode
{
    public class SerializationTests
    {
        [UnityTest]
        public IEnumerator TestSynchronisedObjectListPersistence()
        {
            //allows creating synchronised objects
            GameObject gameObject = new GameObject();
            gameObject.AddComponent<SynchronisedClient>();

            return TestSynchronisedObjectListPersistenceAsync().AsIEnumerator();
        }
        
        private async Task TestSynchronisedObjectListPersistenceAsync()
        {
            //clear old data
            if (PersistentData.TryLoadDatabase("local", out List<PersistentObject> old))
            {
                Debug.Log($"Old saved data: {old.Count}");
                PersistentData.DeleteDatabase("local");
            }
            else
            {
                Debug.Log("No old saved data");
                Assert.IsFalse(PersistentData.DoesDatabaseExists("local"));
            }
            
            TestSynchronisedObject so = new TestSynchronisedObject("123D", 3, 0.5f);
            
            //wait for data to be set
            await so.SetTask;
            
            //ensure persistent data of it exists
            Assert.IsTrue(PersistentData.TryLoadDatabase(so.Id, out _), "Database not saved");

            Database localData = new Database("local", isPersistent: true);
            await localData.Get<TestSynchronisedObject>(so.Id).Set(so);
            
            Assert.IsTrue(PersistentData.TryLoadDatabase(localData.Id, out _), "Local database not saved");
            
            //ensure local data of it exists
            Assert.IsTrue(PersistentData.TryLoadDatabase(localData.Id, out List<PersistentObject> savedObjects));
            Assert.AreEqual(1, savedObjects.Count, "Wrong amount of values saved");
        }
    }
}