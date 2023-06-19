using System;
using System.Collections;
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
using UnityEngine.TestTools;
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
        public static async Task TestRequestReply()
        {
            //start networking
            MessageServer server = null; //new MessageServer("127.0.0.1", GetFreePort());
            server.StartServer();

            MessageClient client = null; //new MessageClient("127.0.0.1", server.Port);
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