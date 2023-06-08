using System.Net;
using Data_Management_for_Unity.Submodules.NetCoreServer;

namespace Data_Management_for_Unity.Runtime.Networking.Messaging
{
    public class MessageServer : TcpServer
    {
        public MessageServer(IPAddress address, int port) : base(address, port)
        {
        }

        public MessageServer(string address, int port) : base(address, port)
        {
        }

        public MessageServer(DnsEndPoint endpoint) : base(endpoint)
        {
        }

        public MessageServer(IPEndPoint endpoint) : base(endpoint)
        {
        }

        protected override TcpSession CreateSession()
        {
            return new MessageSession(this);
        }
    }
}