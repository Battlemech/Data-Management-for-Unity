using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Data_Management_for_Unity.Runtime;
using Data_Management_for_Unity.Runtime.Networking.Messaging;
using Data_Management_for_Unity.Runtime.Networking.Messaging.Client;
using Data_Management_for_Unity.Runtime.Networking.Messaging.Exceptions;
using Data_Management_for_Unity.Runtime.Networking.Messaging.Server;
using Data_Management_for_Unity.Submodules.NetCoreServer;
using NUnit.Framework;
using Debug = UnityEngine.Debug;

namespace Data_Management_for_Unity.Tests.EditMode
{
    public static class NetworkingTests
    {
        private static int _port = Options.DefaultPort;

        public static int GetFreePort()
        {
            return _port++;
        }

        [Test]
        public static void TestSendString()
        {
            TestSend("123431242345134521346231652462456245624562545642654225646254265424566254254625462654");
        }

        [Test]
        public static void TestSendFloat()
        {
            TestSend(21321312321.1232123f);
        }

        [Test]
        public static void TestSendClass()
        {
            TestSend(new TestObject("Detlef"));
        }

        [Test]
        public static async Task TestRequestReply()
        {
            //start networking
            MessageServer server = new MessageServer("127.0.0.1", GetFreePort());
            server.Start();

            MessageClient client = new MessageClient("127.0.0.1", server.Port);
            client.ConnectAsync();
            
            //register server request response
            server.AddCallback<TestRequest, MessageSession>(((request, session) =>
            {
                //return reply to client
                session.Send(new TestReply(request));
            }));

            //wait for reply
            TestRequest request = new TestRequest(123123, 233);
            Stopwatch waitTime = Stopwatch.StartNew();
            TestReply reply = await client.SendRequest<TestRequest, TestReply>(request);
            Debug.Log($"RTT: {waitTime.ElapsedMilliseconds} ms");
            
            Assert.AreEqual(request.A + request.B, reply.Added);
            Assert.AreEqual(request.A * request.B, reply.Multiplied);
            
            //make sure a timeout exception is raised when no reply is received
            Assert.AreEqual(1, server.RemoveCallbacks<TestRequest>());

            waitTime.Restart();
            try
            {
                await client.SendRequest<TestRequest, TestReply>(request, 1000);
                Assert.Fail("Failed to raise expected exception");
            }
            catch (TimedOutException)
            {
                //successfully caught expected exception
                waitTime.Stop();
                Debug.Log($"Raised TimedOutException after: {waitTime.ElapsedMilliseconds} ms!");
            }
            
            Assert.GreaterOrEqual(1000, waitTime.ElapsedMilliseconds);
        }

        private static void TestSend<T>(T expected, int count=100000)
        {
            ManualResetEvent receivedEvent = new ManualResetEvent(false);
            
            //start networking
            MessageServer server = new MessageServer("127.0.0.1", GetFreePort());
            server.Start();

            MessageClient client = new MessageClient("127.0.0.1", server.Port);
            client.ConnectAsync();
            
            /*
             * Test Client send
             */
            
            //give client time to connect
            Assert.IsTrue(client.WaitForConnect());

            //Add callback, waiting for clients message
            server.AddCallback<T, MessageSession>(((arg1, session) =>
            {
                Assert.AreEqual(expected, arg1);
                
                //allow waiting thread to continue
                receivedEvent.Set();
            }));

            long[] sent = new long[count];
            long[] received = new long[count];
            for (int i = 0; i < count; i++)
            {
                //measure elapsed time
                Stopwatch receiveTime = Stopwatch.StartNew();
            
                //client sends message
                client.Send(expected);
                sent[i] = receiveTime.ElapsedMilliseconds;

                Assert.IsTrue(receivedEvent.WaitOne(Options.DefaultTimeout));
                received[i] = receiveTime.ElapsedMilliseconds;
            }

            Debug.Log($"Client: Message sent: {sent.Average()} ms");
            Debug.Log($"Client: Message received: {received.Average()} ms");
            
            /*
             * Test Server multicast
             */
            
            //reset event gate
            receivedEvent.Reset();

            client.AddCallback<T>((obj =>
            {
                Assert.AreEqual(expected, obj);

                //allow waiting thread to continue
                receivedEvent.Set();
            }));

            for (int i = 0; i < count; i++)
            {
                //measure elapsed time
                Stopwatch receiveTime = Stopwatch.StartNew();
                server.Multicast(expected);
                sent[i] = receiveTime.ElapsedMilliseconds;
            
                Assert.IsTrue(receivedEvent.WaitOne(Options.DefaultTimeout));
                received[i] = receiveTime.ElapsedMilliseconds;
            }
            
            Debug.Log($"Server: Message sent: {sent.Average()} ms");
            Debug.Log($"Server: Message received: {received.Average()} ms");
        }
        
        private class TestRequest : Request
        {
            public readonly int A;
            public readonly int B;

            public TestRequest(int a, int b)
            {
                A = a;
                B = b;
            }
        }
        
        private class TestReply : Reply
        {
            public readonly int Added;
            public readonly int Multiplied;
            
            public TestReply(TestRequest request) : base(request)
            {
                Added = request.A + request.B;
                Multiplied = request.A * request.B;
            }
        }
    }
}