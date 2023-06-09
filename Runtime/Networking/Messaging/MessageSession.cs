using Data_Management_for_Unity.Submodules.NetCoreServer;

namespace Data_Management_for_Unity.Runtime.Networking.Messaging
{
    public class MessageSession : TcpSession
    {
        public MessageSession(MessageServer server) : base(server)
        {
        }
    }
}