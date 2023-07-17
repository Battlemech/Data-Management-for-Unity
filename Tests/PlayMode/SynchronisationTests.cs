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
        private List<Database> databases;

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
            databases = new List<Database>() { _database0, _database1, _database2, _database3, _database4 };
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
            _database0.Get<string>(id).Set(value);

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
            return Task.WhenAll(databases.Select(((database, i) => database.Get<int>(id).Set(i + 1))));
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
            
            return Task.WhenAll(databases.Select((database => database.Get<int>(id).Modify((data =>
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
            const int addCount = 200; //todo: implement for addCount = 1000

            //measure elapsed time
            Stopwatch stopwatch = Stopwatch.StartNew();
            
            //start add process concurrently
            yield return TestConcurrentAddAsync(id, addCount).AsIEnumerator();

            //make sure values equal
            yield return ValuesEqual<List<int>>(id, 15000);
            
            //output time
            stopwatch.Stop();
            Debug.Log($"Concurrently added and synchronised {addCount * databases.Count} elements within {stopwatch.ElapsedMilliseconds} ms!");
        }

        private Task TestConcurrentAddAsync(string id, int addCount)
        {
            //start add processes concurrently
            return Task.WhenAll(databases.Select(((database, i) =>
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

        private IEnumerator ValuesEqual<T>(string id, int timeout = Options.DefaultTimeout)
        {
            //make sure value is synchronised in other databases
            yield return TestUtility.AreEqual(true, () =>
            {
                List<T> items = databases.Select((database => database.Get<T>(id).Get())).ToList();
                
                Debug.Log(items.GetContent());

                return items.AreEqual();
            }, "Values Synchronised", timeout);
        }
    }
}