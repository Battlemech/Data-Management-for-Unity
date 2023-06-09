using System;
using System.Net;
using Data_Management_for_Unity.Runtime.Callbacks;
using Data_Management_for_Unity.Runtime.Serializer;
using Data_Management_for_Unity.Submodules.NetCoreServer;

namespace Data_Management_for_Unity.Runtime.Networking.Messaging.Server
{
    public class MessageServer : TcpServer
    {
        protected internal readonly CallbackHandler<Type> CallbackHandler = new CallbackHandler<Type>();

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

        public bool Multicast<T>(T data)
        {
            return base.Multicast(NetworkSerializer.Serialize(Serialization.Serialize(Message.Create(data))));
        }
        
        /// <summary>
        /// Adds a callback, which is invoked whenever an object of the expected type is received
        /// </summary>
        /// <param name="callback">Action invoked when callback is triggered</param>
        /// <param name="name">Name of the callback</param>
        /// <param name="unique">True if callbacks with duplicate names must be prevented</param>
        /// <param name="removeOnError">True if the callbacks must be removed on error</param>
        /// <typeparam name="T">Expected type of object in callback</typeparam>
        /// <returns>True if the callback was added, false if the unique parameter could not be met</returns>
        public bool AddCallback<T>(Action<T> callback, string name = "", bool unique = false, bool removeOnError = false)
        {
            return CallbackHandler.AddCallback(typeof(T), callback, name, unique, removeOnError);
        }

        /// <summary>
        /// Gets the number of callbacks
        /// </summary>
        /// <param name="name">Required name of callbacks, if any</param>
        /// <returns>Number of callbacks matching criterion</returns>
        public int GetCallbackCount<T>(string name=null)
        {
            return CallbackHandler.GetCallbackCount(typeof(T), name);
        }

        /// <summary>
        /// Removes callbacks
        /// </summary>
        /// <param name="name">Required name of callbacks, if any</param>
        /// <returns>Number of callbacks removed</returns>
        public int RemoveCallbacks<T>(string name = null)
        {
            return CallbackHandler.RemoveCallbacks(typeof(T), name);
        }

        protected override TcpSession CreateSession()
        {
            return new MessageSession(this);
        }
    }
}