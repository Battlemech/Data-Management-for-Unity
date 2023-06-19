using System;
using System.Collections.Concurrent;
using System.Net;
using Data_Management_for_Unity.Runtime.Callbacks;
using Data_Management_for_Unity.Runtime.Serializer;
using Data_Management_for_Unity.Submodules.NetCoreServer;

namespace Data_Management_for_Unity.Runtime.Networking.Messaging.Server
{
    public class MessageServer : TcpServer
    {
        private readonly CallbackHandler<Type> _callbackHandler = new CallbackHandler<Type>();

        /// <summary>
        /// Sessions will deserialize received objects on a thread, saving the result here.
        /// The server will later process them on the main thread.
        /// </summary>
        protected internal readonly ConcurrentQueue<Tuple<object, Type, MessageSession>> ReceivedObjects =
            new ConcurrentQueue<Tuple<object, Type, MessageSession>>();

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
        /// <typeparam name="TSession">Expected type of session</typeparam>
        /// <returns>True if the callback was added, false if the unique parameter could not be met</returns>
        public bool AddCallback<T, TSession>(Action<T, TSession> callback, string name = "", bool unique = false, bool removeOnError = false) where TSession : TcpSession
        {
            return _callbackHandler.AddCallback(typeof(T), callback, name, unique, removeOnError);
        }

        /// <summary>
        /// Gets the number of callbacks
        /// </summary>
        /// <param name="name">Required name of callbacks, if any</param>
        /// <returns>Number of callbacks matching criterion</returns>
        public int GetCallbackCount<T>(string name=null)
        {
            return _callbackHandler.GetCallbackCount(typeof(T), name);
        }

        /// <summary>
        /// Removes callbacks
        /// </summary>
        /// <param name="name">Required name of callbacks, if any</param>
        /// <returns>Number of callbacks removed</returns>
        public int RemoveCallbacks<T>(string name = null)
        {
            return _callbackHandler.RemoveCallbacks(typeof(T), name);
        }

        protected override TcpSession CreateSession()
        {
            return new MessageSession(this);
        }

        private void Update()
        {
            //process all received objects
            while (ReceivedObjects.TryDequeue(out Tuple<object, Type, MessageSession> tuple))
            {
                //invoke all callbacks for received object
                _callbackHandler.Invoke(tuple.Item2, tuple.Item1, tuple.Item3);
            }
        }
    }
}