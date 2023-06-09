using System;
using System.Linq;
using Data_Management_for_Unity.Runtime.Callbacks;
using Data_Management_for_Unity.Runtime.Serializer;
using Data_Management_for_Unity.Submodules.NetCoreServer;

namespace Data_Management_for_Unity.Runtime.Networking.Messaging.Server
{
    public class MessageSession : TcpSession
    {
        /// <summary>
        /// Manages the sessions private callbacks.
        /// </summary>
        private readonly CallbackHandler<Type> _sessionCallbacks = new CallbackHandler<Type>();
        
        /// <summary>
        /// Manages the servers callbacks.
        /// </summary>
        private readonly CallbackHandler<Type> _serverCallbacks;
     
        /// <summary>
        /// Tracks received bytes, making sure no partial messages are interpreted
        /// </summary>
        private readonly NetworkSerializer _networkSerializer = new();

        public MessageSession(MessageServer server) : base(server)
        {
            _serverCallbacks = server.CallbackHandler;
        }
        
        /// <summary>
        /// Send data to the client (asynchronous)
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
            return _sessionCallbacks.AddCallback(typeof(T), callback, name, unique, removeOnError);
        }

        /// <summary>
        /// Gets the number of callbacks
        /// </summary>
        /// <param name="name">Required name of callbacks, if any</param>
        /// <returns>Number of callbacks matching criterion</returns>
        public int GetCallbackCount<T>(string name=null)
        {
            return _sessionCallbacks.GetCallbackCount(typeof(T), name);
        }

        /// <summary>
        /// Removes callbacks
        /// </summary>
        /// <param name="name">Required name of callbacks, if any</param>
        /// <returns>Number of callbacks removed</returns>
        public int RemoveCallbacks<T>(string name = null)
        {
            return _sessionCallbacks.RemoveCallbacks(typeof(T), name);
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            //deserialize received bytes, unpacking information about expected length.
            foreach (var message in _networkSerializer.Deserialize(buffer, offset, size).Select(Serialization.Deserialize<Message>))
            {
                //deserialize received object
                object value = message.Deserialize(out Type type);
                
                //invoke sessions callbacks
                _sessionCallbacks.Invoke(type, value);
                
                //invoke server callbacks
                _serverCallbacks.Invoke(type, value);
            }
        }
    }
}