using Data_Management_for_Unity.Runtime.Networking.Synchronising.Client;
using Data_Management_for_Unity.Runtime.Networking.Unity.Messages;
using Data_Management_for_Unity.Runtime.Objects.GameObjects;
using UnityEngine.Events;

namespace Data_Management_for_Unity.Runtime.Networking.Unity.Client
{
    /// <summary>
    /// Wrapper class, exposing internal events to Unity's event system
    /// </summary>
    public class UnityClient : SynchronisedClient
    {
        public new static UnityClient Instance { get; private set; }
        
        public UnityEvent OnConnectingEvent = new UnityEvent();
        public UnityEvent OnConnectedEvent = new UnityEvent();
        public UnityEvent OnDisconnectingEvent = new UnityEvent();
        public UnityEvent OnDisconnectedEvent = new UnityEvent();

        protected override void Awake()
        {
            base.Awake();

            if (Instance == null) Instance = this;

            AddCallback<ShowGameObjectOperation>((operation =>
            {
                //retrieving the game object will create it, if necessary
                operation.Manager.GetGameObject();
            }), mainThread:true);
            AddCallback<DestroyGameObjectOperation>((operation =>
            {
                operation.Reference.OnRemoteDestroyInternal();
            }));
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            //clear instance reference
            if (Instance == this) Instance = null;
        }

        protected override void OnConnecting()
        {
            base.OnConnecting();
            OnConnectingEvent?.Invoke();
        }

        protected override void OnConnected()
        {
            base.OnConnected();
            OnConnectedEvent?.Invoke();
        }

        protected override void OnDisconnecting()
        {
            base.OnDisconnecting();
            OnDisconnectingEvent?.Invoke();
        }

        protected override void OnDisconnected()
        {
            base.OnDisconnected();
            OnDisconnectedEvent?.Invoke();
        }

        /// <summary>
        /// Creates the referenced game object in the network, if it doesn't already exist locally
        /// </summary>
        public bool ShowGameObject(GameObjectManager manager)
        {
            return Send(new ShowGameObjectOperation(manager));
        }

        /// <summary>
        /// Destroys the referenced game object in the network, if it exists locally
        /// </summary>
        public bool DestroyGameObject(GameObjectReference reference)
        {
            return Send(new DestroyGameObjectOperation(reference));
        }
    }
    
    
}