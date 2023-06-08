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
    }
}