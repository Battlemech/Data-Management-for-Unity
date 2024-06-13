using System.Net.Sockets;
using Data_Management_for_Unity.Runtime.Networking.Synchronising.Server;
using Data_Management_for_Unity.Submodules.NetCoreServer;
using UnityEngine.Events;

namespace Data_Management_for_Unity.Runtime.Networking.Unity
{
    /// <summary>
    /// Wrapper class, exposing internal events to Unity's event system
    /// </summary>
    public class UnityServer<T> : SynchronisedServer where T : TcpSession
    {
        public UnityEvent OnStartingEvent;
        public UnityEvent OnStartedEvent;
        public UnityEvent OnStoppingEvent;
        public UnityEvent OnStoppedEvent;
        public UnityEvent<T> OnConnectingEvent;
        public UnityEvent<T> OnConnectedEvent;
        public UnityEvent<T> OnDisconnectingEvent;
        public UnityEvent<T> OnDisconnectedEvent;
        public UnityEvent<SocketError> OnErrorEvent;
        
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