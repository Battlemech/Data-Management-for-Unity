using System.Net.Sockets;
using Data_Management_for_Unity.Runtime.Networking.Unity;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// The client and server game objects aren't destroyed on scene transitions.
/// Thus, they exists in different scenes, but their events can only be accessed via code.
/// This class allows accessing it via Unity's event system.
/// </summary>
public class NetworkEventSystem : MonoBehaviour
{
    public UnityClient client;
    public UnityServer server;
    
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
    public UnityEvent<SocketError> OnServerErrorEvent;
    
    public bool IsConnected => client.IsConnected;
    public bool IsHosting => server.IsStarted;
    
    void Start()
    {
        //find client in scene
        client ??= FindObjectOfType<UnityClient>();
        
        //find server in scene
        server ??= FindObjectOfType<UnityServer>();
        
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
        server.OnErrorEvent.AddListener((error) => OnServerErrorEvent.Invoke(error));
    }
}
