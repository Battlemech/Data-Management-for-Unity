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
            const string localId = "local";
            const string subId = "sub";
            
            List<PersistentObject> saved = await PersistentData2.Load(localId);

            //if no objects were loaded, it must mean no database exists
            if (saved == null)
            {
                Assert.IsFalse(await PersistentData2.DoesDatabaseExists(localId));
            }
            else
            {
                //clear old data
                await PersistentData2.DeleteDatabase(localId);
            }
            
            TestSynchronisedObject so = new TestSynchronisedObject(subId, 3, 0.5f);
            
            //wait for data to be set
            await so.SetTask;
            
            //ensure persistent data of it exists
            Assert.NotNull(PersistentData2.Load(so.Id), "Database not saved");

            //save synchronised object to local database
            Database localData = new Database(localId, isPersistent: true);
            await localData.Get<TestSynchronisedObject>(so.Id).Set(so);
            
            Assert.NotNull(PersistentData2.Load(localData.Id), "Local database not saved");
            
            //ensure local data of it exists
            saved = await PersistentData2.Load(localData.Id);
            Assert.NotNull(saved, "Local database not saved");
            Assert.AreEqual(1, saved.Count, "Wrong amount of values saved");
        }
    }
}