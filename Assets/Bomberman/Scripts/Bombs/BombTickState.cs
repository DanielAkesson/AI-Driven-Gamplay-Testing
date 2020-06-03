using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateKraft;

public class BombTickState : State
{
    private BombController _owner;
    private BombController owner
    {
        get
        {
            if (_owner == null)
                _owner = gameObject.GetComponent<BombController>();
            return _owner;
        }
    }
    public override void Enter()
    {
        owner.IdleWall.SetActive(true);
    }
    public override void StateUpdate()
    {
        owner.CheckIfExplosion();
    }
    public override void Exit()
    {
        owner.IdleWall.SetActive(true);
    }
}
