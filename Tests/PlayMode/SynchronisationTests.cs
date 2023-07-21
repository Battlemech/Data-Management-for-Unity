using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Data_Management_for_Unity.Runtime;
using Data_Management_for_Unity.Runtime.Databases;
using Data_Management_for_Unity.Runtime.Databases.ValueStorages;
using Data_Management_for_Unity.Runtime.Networking.Synchronising.Client;
using Data_Management_for_Unity.Runtime.Networking.Synchronising.Server;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Debug = UnityEngine.Debug;

namespace Data_Management_for_Unity.Tests.PlayMode
{
    public class SynchronisationTests
    {
        private TestClient _client0;
        private TestClient _client1;
        private TestClient _client2;
        private TestClient _client3;
        private TestClient _client4;
        private SynchronisedServer _server;

        private Database _database0;
        private Database _database1;
        private Database _database2;
        private Database _database3;
        private Database _database4;
        private List<Database> _databases;

        [SetUp]
        public void Setup()
        {
            //create game object which holds clients and server
            GameObject gameObject = new GameObject("NetworkManager");
            _client0 = gameObject.AddComponent<TestClient>();
            _client1 = gameObject.AddComponent<TestClient>();
            _client2 = gameObject.AddComponent<TestClient>();
            _client3 = gameObject.AddComponent<TestClient>();
            _client4 = gameObject.AddComponent<TestClient>();
            _server = gameObject.AddComponent<SynchronisedServer>();
            
            Assert.IsFalse(_server.IsStarted);
            Assert.IsFalse(_client0.IsConnected);
            Assert.IsFalse(_client1.IsConnected);
            Assert.IsFalse(_client2.IsConnected);
            Assert.IsFalse(_client3.IsConnected);
            Assert.IsFalse(_client4.IsConnected);
            
            //init clients and server
            _server.Constructor("127.0.0.1", MessagingTests.GetFreePort());
            _client0.Constructor(_server.Address, _server.Port);
            _client1.Constructor(_server.Address, _server.Port);
            _client2.Constructor(_server.Address, _server.Port);
            _client3.Constructor(_server.Address, _server.Port);
            _client4.Constructor(_server.Address, _server.Port);
            
            //create databases
            string id = nameof(SynchronisationTests) + _server.Port;
            _database0 = new Database(id);
            _database1 = new Database(id);
            _database2 = new Database(id);
            _database3 = new Database(id);
            _database4 = new Database(id);
            
            //enable synchronisation of databases before client and server are started
            _database0.Client = _client0;
            _database1.Client = _client1;
            _database0.IsSynchronised = true;
            _database1.IsSynchronised = true;
            
            //start client and server
            _server.StartServer();
            _client0.ConnectAsync();
            _client1.ConnectAsync();
            _client2.ConnectAsync();
            _client3.ConnectAsync();
            _client4.ConnectAsync();
            
            //enable synchronisation of databases while client and server are connecting
            _database2.Client = _client2;
            _database2.IsSynchronised = true;
            
            //make sure client and server are connected
            Assert.IsTrue(_server.IsStarted);
            Assert.IsTrue(_client0.WaitForConnect());
            Assert.IsTrue(_client1.WaitForConnect());
            Assert.IsTrue(_client2.WaitForConnect());
            Assert.IsTrue(_client3.WaitForConnect());
            Assert.IsTrue(_client4.WaitForConnect());
            
            //enable synchronisation of databases after client and server connected
            _database3.Client = _client3;
            _database4.Client = _client4;
            _database3.IsSynchronised = true;
            _database4.IsSynchronised = true;
            
            //init databases list
            _databases = new List<Database>() { _database0, _database1, _database2, _database3, _database4 };
            
            //add main thread runner to execute callbacks from the database
            gameObject.AddComponent<MainThreadRunner>();
        }

        [TearDown]
        public void TearDown()
        {
            Assert.IsTrue(_server.IsStarted);
            Assert.IsTrue(_client0.IsConnected);
            Assert.IsTrue(_client1.IsConnected);
            Assert.IsTrue(_client2.IsConnected);
            Assert.IsTrue(_client3.IsConnected);
            Assert.IsTrue(_client4.IsConnected);
            
            _client0.DisconnectAsync();
            _client1.DisconnectAsync();
            _client2.DisconnectAsync();
            _client3.DisconnectAsync();
            _client4.DisconnectAsync();
            _server.Stop();
            
            Assert.IsFalse(_server.IsStarted);
            Assert.IsFalse(_client0.IsConnected);
            Assert.IsFalse(_client1.IsConnected);
            Assert.IsFalse(_client2.IsConnected);
            Assert.IsFalse(_client3.IsConnected);
            Assert.IsFalse(_client4.IsConnected);
        }

        [Test]
        public void TestSetup()
        {
            Assert.NotNull(_client0);
            Assert.NotNull(_client1);
            Assert.NotNull(_client2);
            Assert.NotNull(_client3);
            Assert.NotNull(_client4);
            Assert.NotNull(_server);
        }

        [UnityTest]
        public IEnumerator TestSimpleSet()
        {
            const string id = nameof(TestSimpleSet);
            const string value = id + "= 'Some beautiful value!'";

            //update local value in database 0
            yield return _database0.Get<string>(id).Set(value).AsIEnumerator();

            //make sure value is synchronised in other databases
            yield return TestUtility.AreEqual(value, () => _database0.Get<string>(id).Get(), "Local set");
            yield return TestUtility.AreEqual(value, () => _database1.Get<string>(id).Get(), "Remote set");
            yield return TestUtility.AreEqual(value, () => _database2.Get<string>(id).Get(), "Remote set");
            yield return TestUtility.AreEqual(value, () => _database3.Get<string>(id).Get(), "Remote set");
            yield return TestUtility.AreEqual(value, () => _database4.Get<string>(id).Get(), "Remote set");
        }
        
        [UnityTest]
        public IEnumerator TestConcurrentSet()
        {
            const string id = nameof(TestConcurrentSet);

            //wait for set process to terminate
            yield return TestConcurrentSetAsync(id).AsIEnumerator();
            
            //make sure value is synchronised in other databases
            yield return ValuesEqual<int>(id);
        }

        private Task TestConcurrentSetAsync(string id)
        {
            //start set processes concurrently
            return Task.WhenAll(_databases.Select(((database, i) => database.Get<int>(id).Set(i + 1))));
        }

        [UnityTest]
        public IEnumerator TestConcurrentModify()
        {
            const string id = nameof(TestConcurrentModify);

            //start chain of modifications, depending on order of results
            yield return TestConcurrentModifyAsync(id).AsIEnumerator();

            yield return TestUtility.AreEqual(1600, () => _database0.Get<int>(id).Get(), "Database 1");
            yield return TestUtility.AreEqual(1600, () => _database1.Get<int>(id).Get(), "Database 2");
            yield return TestUtility.AreEqual(1600, () => _database2.Get<int>(id).Get(), "Database 3");
            yield return TestUtility.AreEqual(1600, () => _database3.Get<int>(id).Get(), "Database 4");
            yield return TestUtility.AreEqual(1600, () => _database4.Get<int>(id).Get(), "Database 5");
            
            //make sure all values were synchronised
            yield return ValuesEqual<int>(id);
        }

        private Task TestConcurrentModifyAsync(string id)
        {
            int invokeCount = 0;
            
            return Task.WhenAll(_databases.Select((database => database.Get<int>(id).Modify((data =>
            {
                //init value to 100
                if (data == default)
                {
                    Debug.Log($"{invokeCount++}=100");
                    return 100;
                }

                //double data
                Debug.Log($"{invokeCount++}={data * 2}");
                return data * 2;
            })))));
        }

        [UnityTest]
        public IEnumerator TestConcurrentAdd()
        {
            const string id = nameof(TestConcurrentAdd);
            const int addCount = 1000; //todo: implement for addCount = 1000

            //measure elapsed time
            Stopwatch stopwatch = Stopwatch.StartNew();
            
            //start add process concurrently
            yield return TestConcurrentAddAsync(id, addCount).AsIEnumerator();

            //make sure values equal
            yield return ValuesEqual<List<int>>(id, 15000);
            
            //output time
            stopwatch.Stop();
            Debug.Log($"Concurrently added and synchronised {addCount * _databases.Count} elements within {stopwatch.ElapsedMilliseconds} ms!");
        }

        private Task TestConcurrentAddAsync(string id, int addCount)
        {
            //start add processes concurrently
            return Task.WhenAll(_databases.Select(((database, i) =>
            {
                Task[] tasks = new Task[addCount];
                
                //start adding elements to list
                for (int j = 0; j < addCount; j++)
                {
                    tasks[j] = database.Get<List<int>>(id).Add(i + j);
                }

                return Task.WhenAll(tasks);
            })));
        }

        [UnityTest]
        public IEnumerator TestConcurrentCollectionOperations()
        {
            const string id = nameof(TestConcurrentCollectionOperations);
            const int addCount = 1000;
            const int removeCount = 200;
            
            //measure elapsed time
            Stopwatch stopwatch = Stopwatch.StartNew();
            
            //start adding and removing objects concurrently
            yield return Task.WhenAll(TestConcurrentAddAsync(id, addCount), TestConcurrentRemoveAsync(id, removeCount)).AsIEnumerator();
            
            //make sure values equal
            yield return ValuesEqual<List<int>>(id, 15000);
            
            //output time
            stopwatch.Stop();
            Debug.Log($"Concurrently added or removed and synchronised {addCount * _databases.Count + removeCount * _databases.Count} elements within {stopwatch.ElapsedMilliseconds} ms!");
        }

        private Task TestConcurrentRemoveAsync(string id, int removeCount)
        {
            return Task.WhenAll(_databases.Select(((database, i) =>
            {
                Task[] tasks = new Task[removeCount];
                
                for (int j = 0; j < removeCount; j++)
                {
                    tasks[j] = database.Get<List<int>>(id).Remove(i + j);
                }

                return Task.WhenAll(tasks);
            })));
        }

        [UnityTest]
        public IEnumerator TestDictionary()
        {
            const string id = nameof(TestDictionary);

            //add keys
            yield return _database0.Get<Dictionary<string, int>>(id).Add(id, 1);
            yield return _database1.Get<Dictionary<string, int>>(id).Add(id + "_", 2);
            
            //make sure they are equal
            yield return ValuesEqual<Dictionary<string, int>>(id);
            
            //remove a key
            yield return _database3.Get<Dictionary<string, int>>(id).RemoveKey<Dictionary<string, int>, string, int>(id);

            //make sure they are equal
            yield return ValuesEqual<Dictionary<string, int>>(id);
        }

        [UnityTest]
        public IEnumerator TestDatabaseCallbacks()
        {
            const string id = nameof(TestDatabaseCallbacks);

            //add callbacks to all databases
            foreach (var database in _databases)
            {
                //when value is modified
                database.AddCallback<int>(id, s =>
                {
                    //append number to other values
                    database.Get<string>(id + "_").Modify((data =>
                    {
                        if (data == null) return "Beginning";
                        return data + s;
                    }));
                });
            }
            
            //invoke callbacks by setting value
            _database0.Get<int>(id).Set(3);
            
            //wait until value was updated
            yield return ValuesAreEqual(id, 3, "First set");
            
            //wait until callbacks were invoked
            yield return ValuesAreEqual(id + "_", "Beginning3333", "Second set");
        }
        
        private IEnumerator ValuesEqual<T>(string id, int timeout = Options.DefaultTimeout)
        {
            //make sure value is synchronised in other databases
            yield return TestUtility.AreEqual(true, () =>
            {
                List<T> items = _databases.Select((database => database.Get<T>(id).Get())).ToList();
                
                Debug.Log(items.GetContent());

                return items.AreEqual();
            }, "Values Synchronised", timeout);
        }

        private IEnumerator ValuesAreEqual<T>(string id, T expected, string name = "Test", int timeout = Options.DefaultTimeout)
        {
            return _databases.Select((database, index) => TestUtility.AreEqual(expected, () => database.Get<T>(id).Get(), name + $" - Database {index}", timeout)).GetEnumerator();
        }
    }
}