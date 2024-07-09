using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatSceneManager : MonoBehaviour
{
    //ui
    public Button hostButton;
    public TMP_Text statusText;
    public TMP_InputField remoteIpInput;
    public TMP_InputField userNameInput;
    public TMP_InputField messageInput;
    public GameObject messageViewContent;
    
    //network
    public ChatClient client;
    public ChatServer server;
    public int port = 25565;

    private void Start()
    {
        hostButton.onClick.AddListener(Host);
        remoteIpInput.onSubmit.AddListener(Connect);
        messageInput.onSubmit.AddListener(SendChatMessage);
        
        //track spawned message objects
        Queue<GameObject> messageObjects = new Queue<GameObject>();
        
        client.OnMessageReceived.AddListener((message =>
        {
            //create text ui from scratch
            GameObject messageObject = new GameObject("Message");
            messageObject.transform.SetParent(messageViewContent.transform);
            TextMeshProUGUI text = messageObject.AddComponent<TextMeshProUGUI>();
            text.text = message.Name + ": " + message.Message;
            
            //add to queue
            messageObjects.Enqueue(messageObject);
            
            //remove oldest message if there are too many
            if (messageObjects.Count > 15)
            {
                Destroy(messageObjects.Dequeue());
            }
        }));
    }

    public void Host()
    {
        server.Constructor(IPAddress.Any, port);
        server.StartServer();
        statusText.text = "Server started";

        //print current connected clients
        server.OnConnectedEvent.AddListener((_ =>
        {
            statusText.text = server.ConnectedSessions + " clients connected";
        }));
        server.OnConnectedEvent.AddListener((_) =>
        {
            statusText.text = server.ConnectedSessions + " clients connected";
        });
        server.OnConnectingEvent.AddListener((_) =>
        {
            Debug.Log("Client is connecting!");
        });
        
        //start to local client
        client.Constructor("127.0.0.1", port);
        client.ConnectAsync();
        
        DisableNetworkConnectionUI();
    }
    
    public void Connect(string remoteIp)
    {
        client.Constructor(remoteIp, port);
        client.ConnectAsync();
        statusText.text = "Client connecting...";
        
        client.OnConnectedEvent.AddListener((() => statusText.text = "Client connected!"));
        
        DisableNetworkConnectionUI();
    }
    
    public void SendChatMessage(string message)
    {
        //send message and clear old input
        client.SendMessage(userNameInput.text, message);
        messageInput.text = "";
        //focus input again
        messageInput.ActivateInputField();
    }
    
    private void DisableNetworkConnectionUI()
    {
        hostButton.interactable = false;
        remoteIpInput.interactable = false;
    }
}
