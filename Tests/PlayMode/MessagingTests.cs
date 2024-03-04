using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Data_Management_for_Unity.Runtime;
using Data_Management_for_Unity.Runtime.Networking.Messaging;
using Data_Management_for_Unity.Runtime.Networking.Messaging.Client;
using Data_Management_for_Unity.Runtime.Networking.Messaging.Exceptions;
using Data_Management_for_Unity.Runtime.Networking.Messaging.Server;
using Data_Management_for_Unity.Runtime.Networking.Synchronising.Server;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Debug = UnityEngine.Debug;

namespace Data_Management_for_Unity.Tests.PlayMode
{
    public class MessagingTests
    {
        private static int _port = Options.DefaultPort;

        private MessageClient _client;
        //Add synchronised server instead of message server to allow adding callbacks
        private SynchronisedServer _server;
        
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
            _server = gameObject.AddComponent<SynchronisedServer>();
            
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

        [UnityTest]
        public IEnumerator TestReplyUnity()
        {
            GameObject serverObject = null;
            GameObject clientObject = null;
            
            //spawn a unity game object when testRequest is received
            _server.AddCallback<TestRequest>((request, session) =>
            {
                //spawn unity game object
                serverObject = new GameObject("Server:"+request.Id);
                Debug.Log(serverObject.name);
                
                //send reply
                session.Send(new TestReply(request));
            });
            
            _client.AddCallback<TestReply>((reply =>
            {
                Debug.Log("Triggering client callback");
                //spawn unity game object
                clientObject = new GameObject("Client:" + reply.Id);
                Debug.Log(clientObject.name);
            }), mainThread:true);
            
            //client sends request
            Task<TestReply> replyTask = _client.SendRequest<TestRequest, TestReply>(new TestRequest(12, 10));

            //wait until reply was received
            yield return replyTask.AsIEnumerator();

            //wait until client processed callback on main thread
            yield return null;
            
            //spawn game Object once reply is received
            GameObject onReplyObject = new GameObject("onReply:" + replyTask.Result.Id);
            Debug.Log(onReplyObject.name);

            Assert.NotNull(serverObject, "Server callback didn't executed successfully");
            Assert.NotNull(clientObject, "Client callback didn't executed successfully");
            Assert.NotNull(onReplyObject, "OnReply callback didn't executed successfully");

            Debug.Log("All objects were spawned successfully");
        }

        [UnityTest]
        public IEnumerator TestServerRequests()
        {
            return TestServerRequestsAsync().AsIEnumerator();
        }

        [UnityTest]
        public IEnumerator TestMessageOrder()
        {
            const int messageCount = 10000;
            
            //create test messages to send
            List<TestMessage> messages = new List<TestMessage>();
            for (int i = 0; i < messageCount; i++)
            {
                messages.Add(new TestMessage(i));
            }
            
            //track order of received messages on server
            int received = 0;
            _server.AddCallback<TestMessage>(((message, session) =>
            {
                //make sure message is received in right order
                Assert.AreEqual(received, message.Id, $"Received {message.Id}, but expected {received}");
                
                //increment number or received messages
                received++;
            }));
            
            //send messages
            foreach (var message in messages)
            {
                _client.Send(message);
            }
            
            //make sure all messages are received
            yield return TestUtility.AreEqual(messageCount, () => received);
        }

        public struct TestMessage
        {
            public readonly int Id;

            public TestMessage(int id)
            {
                Id = id;
            }
        }
        
        
        private async Task TestServerRequestsAsync()
        {
            //reply to server requests
            _client.AddCallback<TestRequest>((request =>
            {
                _client.Send(new TestReply(request));
            }));

            //server requests replies
            TestReply[] replies = await _server.SendRequests<TestRequest, TestReply>(new TestRequest(1, 213), 1000);
            
            //make sure replies were received correctly
            Assert.AreEqual(1, replies.Length);
            Assert.NotNull(replies[0]);
            
            //remove client callback
            _client.RemoveCallbacks<TestRequest>();

            try
            {
                replies = await _server.SendRequests<TestRequest, TestReply>(new TestRequest(1, 213), 1000);
                Assert.Fail("Received client replies");
            }
            catch (TimedOutException)
            {
                Debug.Log("Successfully caught exception");
            }
        }

        private async Task RequestReplyAsync()
        {
            //register server request response
            _server.AddCallback<TestRequest>(((request, session) =>
            {
                //return reply to client
                session.Send(new TestReply(request));
            }));
            
            //wait for reply
            TestRequest request = new TestRequest(123123, 233);
            Stopwatch waitTime = Stopwatch.StartNew();

            TestReply reply;
            try
            {
                reply = await _client.SendRequest<TestRequest, TestReply>(request);
                Debug.Log($"RTT: {waitTime.ElapsedMilliseconds} ms");
            }
            catch (TimedOutException)
            {
                Assert.Fail("Initial request timed out!");
                return;
            }

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
                //make sure unity doesn't cause test to fail after exception is caught
                //todo: figure out what exactly is expected
                LogAssert.Expect(LogType.Exception, new Regex(".{0,}TimedOutException.{0,}"));

                //successfully caught expected exception
                waitTime.Stop();
                Debug.Log($"Raised TimedOutException after: {waitTime.ElapsedMilliseconds} ms!");
            }
            
            Assert.GreaterOrEqual(waitTime.ElapsedMilliseconds, 1000);
                
            //return to avoid exception being thrown
            return;
        }
        
        private IEnumerator TestSend<T>(T expected, int count = 100000)
        {
            bool receivedMessage = false;
            
            /*
             * Test Client send
             */

            //Add callback, waiting for clients message
            _server.AddCallback<T>(((arg1, session) =>
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