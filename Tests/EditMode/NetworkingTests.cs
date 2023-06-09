using System.Threading;
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
        public static void TestNetCoreSync()
        {
            //bytes to receive
            byte[] expected = { 1, 2, 3, 4 };
            var port = GetFreePort();

            //start networking
            var server = new TcpServer("127.0.0.1", port);
            server.Start();

            var client = new TcpClient("127.0.0.1", port);
            client.Connect();

            //give client time to connect
            Thread.Sleep(1000);
            Assert.AreEqual(1, server.ConnectedSessions);

            //send data
            server.Multicast(expected);

            //receive data
            var receiveBuffer = new byte[expected.Length];
            var received = client.Receive(receiveBuffer, 0, expected.Length);

            Assert.AreEqual(expected.Length, received);
            Assert.AreEqual(expected, receiveBuffer);
        }
    }
}