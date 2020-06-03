using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateKraft;

public class BombController : MonoBehaviour
{
    public PlayerController Placer;
    public PlayerController Owner;
    public Arena Arena;
    public int Power;
    public float FuseTime;
    public int Team;
    public GameObject IdleWall;
    public Renderer BombRender;
    public StateMachine StateMachine;

    [HideInInspector] public float CurrentFuseTime;
    void Awake()
    {
        CurrentFuseTime = FuseTime;
        StateMachine.Initialize(this);
        IdleWall.SetActive(false);
    }

    private void FixedUpdate()
    {
        CurrentFuseTime -= Arena.TimeStep;
        StateMachine.Update();
        OcsolateColor();

        Vector3 pos = transform.localPosition;
        if (pos.x < 0.5f || pos.z < 0.5f || pos.x > Arena.WorldSize - 1.5f || pos.z > Arena.WorldSize - 1.5f)
        {
            //Owner.PlayerAgent.AddReward(0.3f);
        }
    }

    public void KickBomb(PlayerController owner)
    {
        Team = owner.Team;
        Owner = owner;
        BombKickState kick = StateMachine.GetState<BombKickState>();
        BombExplodeState explode = StateMachine.GetState<BombExplodeState>();
        if (StateMachine.CurrentState == kick && !kick.AllowKick())
            return;

        if (StateMachine.CurrentState == explode && !BombermanSettings.GetEvno(Arena).JuggleBombs)
            return;

        kick.KickDirection = owner.AimVector.normalized;
        StateMachine.TransitionTo<BombKickState>();
        BombermanSettings.OnBombKick(this);
    }

    public void Hit(BombController bomb)
    {
        //Team = bomb.Team;
        StateMachine.TransitionTo<BombExplodeState>();
    }

    public void CheckIfExplosion()
    {
        if (CurrentFuseTime <= 0)
            StateMachine.TransitionTo<BombExplodeState>();
    }

    public void OcsolateColor()
    {
        float sin = 1;
        float factor = CurrentFuseTime / FuseTime;
        if (CurrentFuseTime > .2f)
            sin = Mathf.Sin(8f / CurrentFuseTime);

        //Change color
        Color color = Color.red * Mathf.Clamp01(sin * sin - factor);
        BombRender.material.SetColor("_EmissionColor", color);
    }

    public bool IsWalkable()
    {
        return StateMachine.CurrentState != StateMachine.GetState<BombTickState>();
    }
}


