using System.Net.Sockets;
using Data_Management_for_Unity.Runtime.Networking.Synchronising.Server;
using Data_Management_for_Unity.Runtime.Networking.Unity;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// The client and server game objects aren't destroyed on scene transitions.
/// Thus, they exists in different scenes, but their events can only be accessed via code.
/// This class allows accessing it via Unity's event system.
/// </summary>
public class AbstractNetworkEventSystem<TSession, TServer> : MonoBehaviour
    where TSession : SynchronisedSession
    where TServer : AbstractUnityServer<TSession>
{
    public UnityClient client;
    public TServer server;
    
    //forward client events
    public UnityEvent OnClientConnectingEvent; 
    public UnityEvent OnClientConnectedEvent;
    public UnityEvent OnClientDisconnectingEvent ;
    public UnityEvent OnClientDisconnectedEvent;
    
    //forward server events
    public UnityEvent OnServerStartingEvent;
    public UnityEvent OnServerStartedEvent;
    public UnityEvent OnServerStoppingEvent;
    public UnityEvent OnServerStoppedEvent;
    public UnityEvent<TSession> OnRemoteConnectingEvent;
    public UnityEvent<TSession> OnRemoteConnectedEvent;
    public UnityEvent<TSession> OnRemoteDisconnectingEvent;
    public UnityEvent<TSession> OnRemoteDisconnectedEvent;
    public UnityEvent<SocketError> OnServerErrorEvent;
    
    public bool IsConnected => client.IsConnected;
    public bool IsHosting => server.IsStarted;
    
    //call before Start() to allow other start scripts to access events
    void Awake()
    {
        //find client in scene
        client ??= FindObjectOfType<UnityClient>();
        
        //find server in scene
        server ??= FindObjectOfType<TServer>();

        if (client == null || server == null)
        {
            Debug.LogWarning("NetworkEventSystem: Client or Server not found in scene. Are you testing locally?");
            return;
        }
        
        //link events
        LinkEvents();
    }

    private void LinkEvents()
    {
        //client events
        client.OnConnectedEvent.AddListener(() => OnClientConnectedEvent.Invoke());
        client.OnConnectingEvent.AddListener(() => OnClientConnectingEvent.Invoke());
        client.OnDisconnectedEvent.AddListener(() => OnClientDisconnectedEvent.Invoke());
        client.OnDisconnectingEvent.AddListener(() => OnClientDisconnectingEvent.Invoke());
        
        //server events
        server.OnStartingEvent.AddListener(() => OnServerStartingEvent.Invoke());
        server.OnStartedEvent.AddListener(() => OnServerStartedEvent.Invoke());
        server.OnStoppedEvent.AddListener(() => OnServerStoppedEvent.Invoke());
        server.OnStoppingEvent.AddListener(() => OnServerStoppingEvent.Invoke());
        server.OnConnectingEvent.AddListener((session) => OnRemoteConnectingEvent.Invoke(session));
        server.OnConnectedEvent.AddListener((session) => OnRemoteConnectedEvent.Invoke(session));
        server.OnDisconnectingEvent.AddListener((session) => OnRemoteDisconnectingEvent.Invoke(session));
        server.OnDisconnectedEvent.AddListener((session) => OnRemoteDisconnectedEvent.Invoke(session));
        server.OnErrorEvent.AddListener((error) => OnServerErrorEvent.Invoke(error));
    }
}
