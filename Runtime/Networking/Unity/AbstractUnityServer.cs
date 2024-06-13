using System.Net.Sockets;
using Data_Management_for_Unity.Runtime.Networking.Synchronising.Server;
using Data_Management_for_Unity.Submodules.NetCoreServer;
using UnityEngine.Events;

namespace Data_Management_for_Unity.Runtime.Networking.Unity
{
    /// <summary>
    /// Wrapper class, exposing internal events to Unity's event system
    /// </summary>
    public abstract class AbstractUnityServer<T> : SynchronisedServer where T : SynchronisedSession
    {
        public UnityEvent OnStartingEvent = new UnityEvent();
        public UnityEvent OnStartedEvent = new UnityEvent();
        public UnityEvent OnStoppingEvent = new UnityEvent();
        public UnityEvent OnStoppedEvent = new UnityEvent();
        public UnityEvent<T> OnConnectingEvent = new UnityEvent<T>();
        public UnityEvent<T> OnConnectedEvent = new UnityEvent<T>();
        public UnityEvent<T> OnDisconnectingEvent = new UnityEvent<T>();
        public UnityEvent<T> OnDisconnectedEvent = new UnityEvent<T>();
        public UnityEvent<SocketError> OnErrorEvent = new UnityEvent<SocketError>();
        
        protected override void OnStarting()
        {
            base.OnStarting();
            OnStartingEvent.Invoke();
        }
        
        protected override void OnStarted()
        {
            base.OnStarted();
            OnStartedEvent.Invoke();
        }
        
        protected override void OnStopping()
        {
            base.OnStopping();
            OnStoppingEvent.Invoke();
        }

        protected override void OnStopped()
        {
            base.OnStopped();
            OnStoppedEvent.Invoke();
        }

        protected override void OnConnecting(TcpSession session)
        {
            base.OnConnecting(session);
            OnConnectingEvent.Invoke((T) session);
        }

        protected override void OnConnected(TcpSession session)
        {
            base.OnConnected(session);
            OnConnectedEvent.Invoke((T) session);
        }
        
        protected override void OnDisconnecting(TcpSession session)
        {
            base.OnDisconnecting(session);
            OnDisconnectingEvent.Invoke((T) session);
        }
        
        protected override void OnDisconnected(TcpSession session)
        {
            base.OnDisconnected(session);
            OnDisconnectedEvent.Invoke((T) session);
        }
        
        protected override void OnError(SocketError error)
        {
            base.OnError(error);
            OnErrorEvent.Invoke(error);
        }
    }
}