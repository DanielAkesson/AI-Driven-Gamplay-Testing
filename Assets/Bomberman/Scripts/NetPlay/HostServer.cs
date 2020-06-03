using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netkraft;
using System;
using Netkraft.Messaging;

public class HostServer : MonoBehaviour
{
    public static HostServer _instance;

    public NetkraftClient hostClient;
    public ClientConnection Player1;
    public ClientConnection Player2;
    
    private void Start()
    {
        _instance = this;
        hostClient = new NetkraftClient(20672);
        hostClient.Host();
    }

    private void Update()
    {
        hostClient.ClientJoinCallback = derp;
        if (hostClient.AllPlayers.Count >= 2)
        {
            Debug.Log("All ready to go");
        }
    }

    private void derp(RequestJoin arg1, ClientConnection arg2)
    {
        if (Player1 == null)
            Player1 = arg2;
        else if (Player2 == null)
            Player2 = arg2;
        Debug.Log("Observer Joined");
    }
}
