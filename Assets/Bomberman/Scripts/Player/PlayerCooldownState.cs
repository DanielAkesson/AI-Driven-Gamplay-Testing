using System.Collections;
using StateKraft;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCooldownState : State
{
    private float cooldownTime = 0;
    private State returnState;
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
    public void Setup(State returnState, float cooldownTime)
    {
        this.cooldownTime = cooldownTime;
        this.returnState = returnState;
    }
    public override void StateUpdate()
    {
        if (cooldownTime <= 0)
            StateMachine.TransitionTo(returnState);
        cooldownTime -= Arena.TimeStep;
    }
}
