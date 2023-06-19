using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Data_Management_for_Unity.Runtime;
using Data_Management_for_Unity.Runtime.Networking.Messaging.Client;
using Data_Management_for_Unity.Runtime.Networking.Messaging.Exceptions;
using Data_Management_for_Unity.Runtime.Networking.Messaging.Server;
using Data_Management_for_Unity.Tests.EditMode;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Debug = UnityEngine.Debug;

namespace Data_Management_for_Unity.Tests.PlayMode
{
    public class NetworkTests
    {
        private static int _port = Options.DefaultPort;

        private MessageClient _client;
        private MessageServer _server;
        
        public static int GetFreePort()
        {
            return _port++;
        }

        [SetUp]
        public void Setup()
        {
            //create game object which holds client and server
            GameObject gameObject = new GameObject("NetworkManager");
            _client = gameObject.AddComponent<MessageClient>();
            _server = gameObject.AddComponent<MessageServer>();
            
            Assert.IsFalse(_server.IsStarted);
            Assert.IsFalse(_client.IsConnected);
            
            //init client and server
            _server.Constructor("127.0.0.1", GetFreePort());
            _client.Constructor(_server.Address, _server.Port);
            
            //start client and server
            _server.StartServer();
            _client.ConnectAsync();
            
            //make sure client and server are connected
            Assert.IsTrue(_server.IsStarted);
            Assert.IsTrue(_client.WaitForConnect());
        }

        [TearDown]
        public void TearDown()
        {
            Assert.IsTrue(_server.IsStarted);
            Assert.IsTrue(_client.IsConnected);
            
            _client.DisconnectAsync();
            _server.Stop();
            
            Assert.IsFalse(_server.IsStarted);
            Assert.IsFalse(_client.IsConnected);
        }

        [Test]
        public void TestSetup()
        {
            Assert.NotNull(_client);
            Assert.NotNull(_server);
        }

        [UnityTest]
        public IEnumerator TestSendString()
        {
            return TestSend("123431242345134521346231652462456245624562545642654225646254265424566254254625462654");
        }

        [UnityTest]
        public IEnumerator TestSendFloat()
        {
            return TestSend(21321312321.1232123f);
        }

        [UnityTest]
        public IEnumerator TestSendClass()
        {
            return TestSend(new TestObject("Detlef"));
        }

        [UnityTest]
        public IEnumerator TestRequestReply()
        {
            return RequestReplyAsync().AsIEnumerator();
        }
        
        public async Task RequestReplyAsync()
        {
            //register server request response
            _server.AddCallback<TestRequest, MessageSession>(((request, session) =>
            {
                //return reply to client
                session.Send(new TestReply(request));
            }));
            
            //wait for reply
            TestRequest request = new TestRequest(123123, 233);
            Stopwatch waitTime = Stopwatch.StartNew();

            TestReply reply = await _client.SendRequest<TestRequest, TestReply>(request);
            Debug.Log($"RTT: {waitTime.ElapsedMilliseconds} ms");
            
            Assert.AreEqual(request.A + request.B, reply.Added);
            Assert.AreEqual(request.A * request.B, reply.Multiplied);
            
            //make sure a timeout exception is raised when no reply is received
            Assert.AreEqual(1, _server.RemoveCallbacks<TestRequest>());

            waitTime.Restart();
            try
            {
                await _client.SendRequest<TestRequest, TestReply>(request, 1000);
                Assert.Fail("Failed to raise expected exception");
            }
            catch (TimedOutException)
            {
                //successfully caught expected exception
                waitTime.Stop();
                Debug.Log($"Raised TimedOutException after: {waitTime.ElapsedMilliseconds} ms!");
            }
            
            Assert.GreaterOrEqual(waitTime.ElapsedMilliseconds, 1000);
        }
        
        private IEnumerator TestSend<T>(T expected, int count = 100000)
        {
            bool receivedMessage = false;
            
            /*
             * Test Client send
             */

            //Add callback, waiting for clients message
            _server.AddCallback<T, MessageSession>(((arg1, session) =>
            {
                Assert.AreEqual(expected, arg1);
                
                //allow waiting thread to continue
                receivedMessage = true;
            }));

            long[] sent = new long[count];
            long[] received = new long[count];
            for (int i = 0; i < count; i++)
            {
                //measure elapsed time
                Stopwatch receiveTime = Stopwatch.StartNew();
            
                //client sends message
                _client.Send(expected);
                sent[i] = receiveTime.ElapsedMilliseconds;

                //wait for message to be received
                while (!receivedMessage)
                {
                    yield return null;
                }
                
                received[i] = receiveTime.ElapsedMilliseconds;
            }

            Debug.Log($"Client: Message sent: {sent.Average()} ms");
            Debug.Log($"Client: Message received: {received.Average()} ms");
            
            /*
             * Test Server multicast
             */
            
            //reset event gate
            receivedMessage = false;

            _client.AddCallback<T>((obj =>
            {
                Assert.AreEqual(expected, obj);

                //allow waiting thread to continue
                receivedMessage = true;
            }));

            for (int i = 0; i < count; i++)
            {
                //measure elapsed time
                Stopwatch receiveTime = Stopwatch.StartNew();
                _server.Multicast(expected);
                sent[i] = receiveTime.ElapsedMilliseconds;
            
                //wait for message to be received
                while (!receivedMessage)
                {
                    yield return null;
                }
                
                received[i] = receiveTime.ElapsedMilliseconds;
            }
            
            Debug.Log($"Server: Message sent: {sent.Average()} ms");
            Debug.Log($"Server: Message received: {received.Average()} ms");
        }
    }
}