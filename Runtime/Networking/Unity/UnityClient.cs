using Data_Management_for_Unity.Runtime.Networking.Synchronising.Client;
using UnityEngine.Events;

namespace Data_Management_for_Unity.Runtime.Networking.Unity
{
    /// <summary>
    /// Wrapper class, exposing internal events to Unity's event system
    /// </summary>
    public class UnityClient : SynchronisedClient
    {
        public UnityEvent OnConnectingEvent;
        public UnityEvent OnConnectedEvent;
        public UnityEvent OnDisconnectingEvent;
        public UnityEvent OnDisconnectedEvent;
        
        protected override void OnConnecting()
        {
            base.OnConnecting();
            OnConnectingEvent.Invoke();
        }

        protected override void OnConnected()
        {
            base.OnConnected();
            OnConnectedEvent.Invoke();
        }

        protected override void OnDisconnecting()
        {
            base.OnDisconnecting();
            OnDisconnectingEvent.Invoke();
        }

        protected override void OnDisconnected()
        {
            base.OnDisconnected();
            OnDisconnectedEvent.Invoke();
        }
    }
    
    
}