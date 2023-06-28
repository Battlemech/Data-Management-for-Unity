using System.Collections;
using System.Diagnostics;
using System.Threading.Tasks;
using Data_Management_for_Unity.Runtime;
using Data_Management_for_Unity.Runtime.Databases;
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
        private SynchronisedClient _client0;
        private SynchronisedClient _client1;
        private SynchronisedClient _client2;
        private SynchronisedClient _client3;
        private SynchronisedClient _client4;
        private SynchronisedServer _server;

        private Database _database0;
        private Database _database1;
        private Database _database2;
        private Database _database3;
        private Database _database4;

        [SetUp]
        public void Setup()
        {
            //create game object which holds clients and server
            GameObject gameObject = new GameObject("NetworkManager");
            _client0 = gameObject.AddComponent<SynchronisedClient>();
            _client1 = gameObject.AddComponent<SynchronisedClient>();
            _client2 = gameObject.AddComponent<SynchronisedClient>();
            _client3 = gameObject.AddComponent<SynchronisedClient>();
            _client4 = gameObject.AddComponent<SynchronisedClient>();
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

            //update local value in database 0
            _database0.Get<int>(id).Set(10);
            _database1.Get<int>(id).Set(1);
            _database2.Get<int>(id).Set(2);
            _database3.Get<int>(id).Set(3);
            _database4.Get<int>(id).Set(4);

            //make sure value is synchronised in other databases
            yield return TestUtility.AreEqual(true, () =>
            {
                int a = _database0.Get<int>(id).Get();
                int b = _database1.Get<int>(id).Get();
                int c = _database2.Get<int>(id).Get();
                int d = _database3.Get<int>(id).Get();
                int e = _database4.Get<int>(id).Get();
                
                Debug.Log($"{a}, {b}, {c}, {d}, {e}");
                
                return a == b && b == c && c == d && d == e;
            }, "Values Synchronised");
        }
    }
}