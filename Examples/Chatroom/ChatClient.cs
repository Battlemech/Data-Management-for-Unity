using System;
using System.Collections;
using System.Collections.Generic;
using Data_Management_for_Unity.Examples.Chatroom;
using Data_Management_for_Unity.Runtime.Networking.Unity;
using UnityEngine;
using UnityEngine.Events;

public class ChatClient : UnityClient
{
    public UnityEvent<ChatMessage> OnMessageReceived = new UnityEvent<ChatMessage>();
    
    protected override void Awake()
    {
        base.Awake();

        AddCallback<ChatMessage>((message =>
        {
            OnMessageReceived?.Invoke(message);
        }), mainThread:true);
    }

    public bool SendMessage(string userName, string message)
    {
        return Send(new ChatMessage(userName, message));
    }
}
