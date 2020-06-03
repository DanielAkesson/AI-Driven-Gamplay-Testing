using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateKraft;
using System.Linq;

public class PlayerDeadState : State
{
    public float ShoWDeathTime;
    private float currentShowDeathTime;
    private bool restart;
    private PlayerController _owner;
    private PlayerController owner
    {
        get
        {
            if (_owner == null)
                _owner = gameObject.GetComponent<PlayerController>();
            return _owner;
        }
    }
    public override void Enter()
    {
        restart = false;
        currentShowDeathTime = 0;
        owner.PlayerRender.material.color = Color.black;
        BombermanSettings.OnPlayerDead(owner);
    }
    public override void StateUpdate()
    {
        currentShowDeathTime += Arena.TimeStep;
        if (currentShowDeathTime >= ShoWDeathTime && !restart)
        {
            //BombermanSettings.OnPlayerDead(owner);
            restart = true;
        }
    }
}
