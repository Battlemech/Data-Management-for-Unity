using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Data_Management_for_Unity.Runtime.Serializer;
using Data_Management_for_Unity.Submodules.NetCoreServer;

namespace Data_Management_for_Unity.Runtime.Networking.Messaging.Client
{
    public partial class MessageClient : TcpClient
    {
        //tracks received bytes, making sure no partial messages are interpreted
        private readonly NetworkSerializer _networkSerializer = new();

        //allows waiting until client is connected
        private readonly ManualResetEvent _connectEvent = new ManualResetEvent(false);

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

        /// <summary>
        /// Send data to the server (asynchronous)
        /// </summary>
        /// <returns>'true' if the data was successfully sent, 'false' if the client is not connected</returns>
        public bool Send<T>(T data)
        {
            //1) Wrap data in message
            //2) Serialize message as bytes
            //3) Wrap serialized message with additional information about its length to ensure no partial messages are received
            return base.SendAsync(NetworkSerializer.Serialize(Serialization.Serialize(Message.Create(data))));
        }

        /// <summary>
        /// Waits until the client is connected.
        /// </summary>
        /// <param name="timeout">Wait time in ms</param>
        /// <returns>True if the client connected, otherwise false</returns>
        public bool WaitForConnect(int timeout = Options.DefaultTimeout)
        {
            return _connectEvent.WaitOne(timeout);
        }

        protected override void OnConnected()
        {
            _connectEvent.Set();
        }

        protected override void OnDisconnecting()
        {
            _connectEvent.Reset();
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
                _callbackHandler.Invoke(type, value);
            }
        }
    }
}