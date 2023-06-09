using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Data_Management_for_Unity.Runtime.Serializer;
using Data_Management_for_Unity.Submodules.NetCoreServer;

namespace Data_Management_for_Unity.Runtime.Networking.Messaging.Client
{
    public partial class MessageClient : TcpClient
    {
        //tracks received bytes, making sure no partial messages are interpreted
        private readonly NetworkSerializer _networkSerializer = new();

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

        public bool Send<T>(T data)
        {
            //1) Wrap data in message
            //2) Serialize message as bytes
            //3) Wrap serialized message with additional information about its length to ensure no partial messages are received
            return SendAsync(NetworkSerializer.Serialize(Serialization.Serialize(Message.Create(data))));
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            //deserialize received bytes, unpacking information about expected length.
            foreach (var bytes in _networkSerializer.Deserialize(buffer, offset, size))
            {
                //deserialize received message
                Message message = Serialization.Deserialize<Message>(bytes);
                
                //deserialize received object
                object value = message.Deserialize(out Type type);
                
                //invoke callbacks
                _callbackHandler.InvokeCallbacks(type, value);
            }
        }
    }
}