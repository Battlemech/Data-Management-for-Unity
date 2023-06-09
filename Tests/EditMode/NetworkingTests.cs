using System.Threading;
using Data_Management_for_Unity.Runtime;
using Data_Management_for_Unity.Runtime.Networking.Messaging.Client;
using Data_Management_for_Unity.Runtime.Networking.Messaging.Server;
using Data_Management_for_Unity.Submodules.NetCoreServer;
using NUnit.Framework;

namespace Data_Management_for_Unity.Tests.EditMode
{
    public static class NetworkingTests
    {
        private static int _port = 8000;

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

        private static void TestSend<T>(T expected)
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
            server.AddCallback<T>((s =>
            {
                Assert.AreEqual(expected, s);
                
                //allow waiting thread to continue
                receivedEvent.Set();
            }));
            
            //client sends message
            client.Send(expected);
            
            Assert.IsTrue(receivedEvent.WaitOne(Options.DefaultTimeout));
            
            /*
             * Test Server send
             */
            
            //reset event gate
            receivedEvent.Reset();

            client.AddCallback<T>((obj =>
            {
                Assert.AreEqual(expected, obj);

                //allow waiting thread to continue
                receivedEvent.Set();
            }));

            server.Multicast(expected);
            
            Assert.IsTrue(receivedEvent.WaitOne(Options.DefaultTimeout));
        }
    }
}