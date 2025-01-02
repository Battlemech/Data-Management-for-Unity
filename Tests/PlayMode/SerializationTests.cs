using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data_Management_for_Unity.Runtime;
using Data_Management_for_Unity.Runtime.Databases;
using Data_Management_for_Unity.Runtime.Networking.Synchronising.Client;
using Data_Management_for_Unity.Runtime.Persistence3;
using Data_Management_for_Unity.Runtime.Serializer;
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

        [UnityTest]
        public IEnumerator TestDictionaryReference()
        {
            //allows creating synchronised objects
            GameObject gameObject = new GameObject();
            gameObject.AddComponent<SynchronisedClient>();
            
            return TestDictionaryReferenceAsync().AsIEnumerator();
        }
        
        private async Task TestDictionaryReferenceAsync()
        {
            //init with list of TestCoordinates
            List<TestCoordinate> coordinates = new List<TestCoordinate>()
            {
                new TestCoordinate(1, 2),
                new TestCoordinate(3, 4),
                new TestCoordinate(5, 6),
                new TestCoordinate(7, 8),
                new TestCoordinate(9, 10),
            };
            
            //init with dictionary of TestCoordinates
            Dictionary<TestCoordinate, TestTile> dict = new Dictionary<TestCoordinate, TestTile>()
            {
                { coordinates[0], new TestTile(coordinates[0]) },
                { coordinates[1], new TestTile(coordinates[1]) },
                { coordinates[2], new TestTile(coordinates[2]) },
                { coordinates[3], new TestTile(coordinates[3]) },
                { coordinates[4], new TestTile(coordinates[4]) },
            };
            
            //save synchronised object to local database
            var copy = SerializationPCK.Copy(dict);
            
            Assert.AreEqual(dict, copy, "Dictionary not copied correctly");
            Debug.Log("Copied count: " + copy.Count);
            
            //ensure the correct coordinates are referenced in copy
            foreach (var pair in copy)
            {
                Assert.AreEqual(pair.Key, pair.Value.Coordinate.Get(), "Coordinate mismatch");
            }
            
            //ensure the dict can be saved persistently
            PersistentData.CreateDatabase("dict");
            await PersistentData.Save("dict", "dict", SerializationPCK.Serialize(dict), typeof(Dictionary<TestCoordinate, TestTile>), 0);
            
            //ensure the dict can be loaded persistently
            Assert.IsTrue(PersistentData.TryLoad("dict", "dict", out PersistentObject po), "Persistent object not found");
            if (po.Deserialize() is Dictionary<TestCoordinate, TestTile> loadedDict)
            {
                //ensure the amount of saved entries is equal
                Assert.AreEqual(dict.Count, loadedDict.Count, "Dictionary count mismatch");
                Debug.Log("Loaded count: " + loadedDict.Count);
                
                //ensure the loaded dict equals the original
                Assert.AreEqual(dict, loadedDict, "Dictionary not loaded correctly");
            }
            else
            {
                Assert.Fail("Deserialized object is not a dictionary");
            }
            
        }
    }
}