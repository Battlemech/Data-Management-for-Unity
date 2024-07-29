using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data_Management_for_Unity.Runtime;
using Data_Management_for_Unity.Runtime.Databases;
using Data_Management_for_Unity.Runtime.Networking.Synchronising.Client;
using Data_Management_for_Unity.Runtime.Persistence3;
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
            const string localId = "local";
            const string subId = "sub";
            
            List<PersistentObject> saved = PersistentData.Load(localId);

            //if no objects were loaded, it must mean no database exists
            if (saved == null)
            {
                Assert.IsFalse(PersistentData.DoesDatabaseExists(localId));
            }
            else
            {
                //clear old data
                PersistentData.DeleteDatabase(localId);
            }
            
            TestDatabaseReference so = new TestDatabaseReference(subId, 3, 0.5f);
            
            //wait for data to be set
            await so.SetTask;
            
            //ensure persistent data of it exists
            Assert.NotNull(PersistentData.Load(so.Id), "Database not saved");

            //save synchronised object to local database
            Database localData = new Database(localId, isPersistent: true);
            await localData.Get<TestDatabaseReference>(so.Id).Set(so);
            
            Assert.NotNull(PersistentData.Load(localData.Id), "Local database not saved");
            
            //ensure local data of it exists
            saved = PersistentData.Load(localData.Id);
            Assert.NotNull(saved, "Local database not saved");
            Assert.AreEqual(1, saved.Count, "Wrong amount of values saved");
        }
    }
}