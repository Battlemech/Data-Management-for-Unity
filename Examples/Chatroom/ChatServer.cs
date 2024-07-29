using System.Collections;
using System.Collections.Generic;
using Data_Management_for_Unity.Runtime.Networking.Unity;
using Data_Management_for_Unity.Runtime.Networking.Unity.Server;
using Data_Management_for_Unity.Submodules.NetCoreServer;
using UnityEngine;

public class ChatServer : UnityServer
{
    protected override TcpSession CreateSession()
    {
        return new ChatSession(this);
    }
}
