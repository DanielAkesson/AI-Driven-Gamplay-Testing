using Netkraft;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Messenging;

public class Client : MonoBehaviour
{
    public static bool GO = false;
    public static Client _instance;
    public NetkraftClient client;

    private PlayerInput input = new PlayerInput();
    void Start()
    {
        _instance = this;
        client = new NetkraftClient(Random.Range(1000, 4000));
        client.Join("127.0.0.1", 20672);
    }

    private void Update()
    {
        if (!GO) return;
        //Recive server messages
        client.ReceiveTick();

        //Send back
        PullController();
        client.AddToQueue(input);
        client.SendQueue();
    }

    public void PullController()
    {
        input.Move = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        input.Aim = new Vector3(Input.GetAxisRaw("AimHorizontal"), 0, Input.GetAxisRaw("AimVertical"));
        input.Bomb = Input.GetButton("Bomb");
        input.Kick = Input.GetButton("Kick");
    }
}
