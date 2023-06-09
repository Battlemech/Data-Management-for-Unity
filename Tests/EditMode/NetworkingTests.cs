using System.Threading;
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
            string expected = "123456789";
            string received = null;
            
            //start networking
            MessageServer server = new MessageServer("127.0.0.1", GetFreePort());
            server.Start();

            MessageClient client = new MessageClient("127.0.0.1", server.Port);
            client.ConnectAsync();
            
            //give client time to connect
            Assert.IsTrue(client.WaitForConnect());

            //Add callback, waiting for clients message
            server.AddCallback<string>((s => received = s));
            
            //client sends message
            client.Send(expected);
            
            //Wait for message to arrive
            Thread.Sleep(1000);
            
            Assert.AreEqual(expected, received);
        }
    }
}