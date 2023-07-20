using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Data_Management_for_Unity.Runtime.Callbacks;
using Data_Management_for_Unity.Runtime.Networking.Synchronising.Server;
using Data_Management_for_Unity.Runtime.Serializer;
using Data_Management_for_Unity.Submodules.NetCoreServer;
using UnityEngine;

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
        /// Multicasts the object to all sessions except the specified one
        /// </summary>
        public bool MulticastToOthers<T>(T data, MessageSession session)
        {
            if (!IsStarted) return false;
            
            //serialize value which needs to be sent
            byte[] toSend = NetworkSerializer.Serialize(Serialization.Serialize(Message.Create(data)));

            //loop through all sessions
            foreach (var messageSession in Sessions.Values.Cast<MessageSession>())
            {
                //skip session which needs to be excluded
                if(messageSession.Id == session.Id) continue;
                
                messageSession.SendAsync(toSend);
            }

            return true;
        }

        /// <summary>
        /// Adds a callback, which is invoked whenever an object of the expected type is received.
        /// Callbacks added here are executed on the main thread.
        /// </summary>
        /// <remarks>If a callback with the same type was added to the server's receiving session, the servers callback will not be invoked!</remarks>
        /// <param name="callback">Action invoked when callback is triggered</param>
        /// <param name="name">Name of the callback</param>
        /// <param name="unique">True if callbacks with duplicate names must be prevented</param>
        /// <param name="removeOnError">True if the callbacks must be removed on error</param>
        /// <typeparam name="T">Expected type of object in callback</typeparam>
        /// <typeparam name="TSession">Expected type of session</typeparam>
        /// <returns>True if the callback was added, false if the unique parameter could not be met</returns>
        public bool AddCallback<T, TSession>(Action<T, TSession> callback, string name = "", bool unique = false, bool removeOnError = false)
            //Make sure overwriting sessions use SynchronisedSessions, not only MessageSessions
            where TSession : SynchronisedSession
        {
            return _callbackHandler.AddCallback(typeof(T), callback, name, unique, removeOnError);
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
        public bool AddCallback<T>(Action<T, SynchronisedSession> callback, string name = "", bool unique = false,
            bool removeOnError = false) 
        {
            //Make sure overwriting sessions use SynchronisedSessions, not only MessageSessions
            return AddCallback<T, SynchronisedSession>(callback, name, unique, removeOnError);
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

        public Task<TReply[]> SendRequests<TRequest, TReply>(TRequest request, int timeout = Options.DefaultTimeout)
            where TRequest : Request where TReply : Reply
        {
            //get all sessions, casting them to MessageSession to allow accessing request function
            return Task.WhenAll(Sessions.Values.Cast<MessageSession>()
                //send the request from all sessions
                .Select((session => session.SendRequest<TRequest, TReply>(request, timeout))));
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
                if(_callbackHandler.Invoke(tuple.Item2, tuple.Item1, tuple.Item3) > 0) continue;
                
                Debug.LogWarning($"Server: Received object of type {tuple.Item2} didn't trigger any callbacks!");
            }
        }
    }
}