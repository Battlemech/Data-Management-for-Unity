using System.Threading;
using Data_Management_for_Unity.Runtime.Networking.Messaging;
using Data_Management_for_Unity.Submodules.NetCoreServer;
using NUnit.Framework;

namespace Data_Management_for_Unity.Tests.EditMode
{
    public static class NetworkingTests
    {
        public static int GetFreePort()
        {
            return _port++;
        }
        private static int _port = 8000;

        [Test]
        public static void TestNetCoreSync()
        {
            //bytes to receive
            byte[] expected = new byte[] { 1, 2, 3, 4 };
            int port = GetFreePort();
            
            //start networking
            TcpServer server = new TcpServer("127.0.0.1", port);
            server.Start();
            
            TcpClient client = new TcpClient("127.0.0.1", port);
            client.Connect();

            //give client time to connect
            Thread.Sleep(1000);
            Assert.AreEqual(1, server.ConnectedSessions);
            
            //send data
            server.Multicast(expected);
            
            //receive data
            byte[] receiveBuffer = new byte[expected.Length];
            long received = client.Receive(receiveBuffer, 0 ,expected.Length);

            Assert.AreEqual(expected.Length, received);
            Assert.AreEqual(expected, receiveBuffer);
        }
    }
}