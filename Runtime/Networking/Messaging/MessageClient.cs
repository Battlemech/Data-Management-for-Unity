using System;
using System.Net;
using Data_Management_for_Unity.Submodules.NetCoreServer;

namespace Data_Management_for_Unity.Runtime.Networking.Messaging
{
    public class MessageClient : TcpClient
    {
        public MessageClient(IPAddress address, int port) : base(address, port)
        {
        }

        public MessageClient(string address, int port) : base(address, port)
        {
        }

        public MessageClient(DnsEndPoint endpoint) : base(endpoint)
        {
        }

        public MessageClient(IPEndPoint endpoint) : base(endpoint)
        {
        }

        public T Send<T>(T data)
        {
            //todo: Network Serializer
            throw new NotImplementedException();
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            //todo: Network Serializer
            throw new NotImplementedException();
        }
    }
}