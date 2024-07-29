using System.Collections;
using System.Collections.Generic;
using Data_Management_for_Unity.Examples.Chatroom;
using Data_Management_for_Unity.Runtime.Networking.Synchronising.Server;
using Data_Management_for_Unity.Runtime.Networking.Unity.Server;
using UnityEngine;

public class ChatSession : UnitySession
{
    public ChatSession(ChatServer server) : base(server)
    {
        AddCallback<ChatMessage>((message =>
        {
            server.Multicast(message);
        }));
    }
}
