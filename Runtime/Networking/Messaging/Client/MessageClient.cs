using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Data_Management_for_Unity.Runtime.Serializer;
using Data_Management_for_Unity.Submodules.NetCoreServer;
using UnityEngine;

namespace Data_Management_for_Unity.Runtime.Networking.Messaging.Client
{
    public partial class MessageClient : TcpClient
    {
        //tracks received bytes, making sure no partial messages are interpreted
        private readonly NetworkSerializer _networkSerializer = new();

        //allows waiting until client is connected
        private readonly ManualResetEvent _connectEvent = new ManualResetEvent(false);

        /// <summary>
        /// Client will deserialize received objects on a thread, saving the result here.
        /// The main thread will process received objects.
        /// </summary>
        private readonly ConcurrentQueue<Tuple<object, Type>> _receivedObjects =
            new ConcurrentQueue<Tuple<object, Type>>();

        /// <summary>
        /// Send data to the server (asynchronous)
        /// </summary>
        /// <returns>'true' if the data was successfully sent, 'false' if the client is not connected</returns>
        public bool Send(object o)
        {
            //1) Wrap data in serializedObject, preserving its type
            //2) Serialize message as bytes
            //3) Wrap serialized message with additional information about its length to ensure no partial messages are received
            return base.SendAsync(NetworkSerializer.Serialize(SerializationPCK.Serialize(new SerializedObject(o))));
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
            try
            {
                //deserialize received bytes, unpacking information about expected length.          //deserialize received message
                foreach (var message in _networkSerializer.Deserialize(buffer, offset, size).Select(SerializationPCK.Deserialize<SerializedObject>))
                {
                    //deserialize received object
                    object value = message.Deserialize(out Type type);

                    //invoke all threaded callbacks
                    _threadedCallbacks.Invoke(type, value);
                    
                    //don't notify Unity's main thread if no callback were added
                    if(_mainThreadCallbacks.GetCallbackCount(type) == 0) continue;
                
                    //save deserialized objects to be processed on main thread
                    _receivedObjects.Enqueue(new Tuple<object, Type>(value, type));
                }
            }
            catch (Exception e)
            {
                //log any exceptions which occur, but don't terminate receiving thread
                Debug.LogException(e);
            }
        }

        protected virtual void Update()
        {
            //process all received objects
            while (_receivedObjects.TryDequeue(out Tuple<object, Type> tuple))
            {
                //invoke all callbacks for received object
                _mainThreadCallbacks.Invoke(tuple.Item2, tuple.Item1);
            }
        }
    }
}