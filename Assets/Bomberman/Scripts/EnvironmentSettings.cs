using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Environment/Settings")]
public class EnvironmentSettings : ScriptableObject
{
    [System.Serializable]
    public class PlayerControllerEvent : UnityEvent<PlayerController> { }
    [System.Serializable]
    public class PlayerAgentEvent : UnityEvent<PlayerAgent> { }
    [System.Serializable]
    public class ArenaEvent : UnityEvent<Arena> { }
    [System.Serializable]
    public class BombControllerEvent : UnityEvent<BombController> { }
    [Header("Game Over")]
    public PlayerControllerEvent OnPlayerDead;
    [Header("Update")]
    public PlayerAgentEvent OnAgentUpdate;
    public ArenaEvent OnArenaUpdate;
    public BombControllerEvent OnBombSpawn;
    public BombControllerEvent OnBombKick;
    public float UpdateReward;
    public float DrawReward;
    public float SpawnBombReward;
    public float KickBombReward;
    public float InRangeReward;
    public float InsideBoxReward;
    public LayerMask BoxLayer;

    [Header("Exploits and bugs")]
    public bool JuggleBombs = false;
    public bool CellLineProtection = false;
    public float CellLineProtectionSize = .1f;
    public bool PowerStacking = false;
    public int PowerStackingNumber = 3;
    public bool PowerDoNothing = false;
    public bool BugColliderBoxes = false;
    public bool PlayerColliderIgnoredOnBombDrop = false;
    public float PlayerColliderIgnoredOnBombDropTime = .04f;

    private Dictionary<PlayerAgent, float> OutsideReward;

    private bool ending = false;

    private void ResetEnv(Arena arena)
    {
        ending = false;
        stepCounter = 0;
        arena.Generate();
        OutsideReward = new Dictionary<PlayerAgent, float>();
        arena.Players.ForEach(player => OutsideReward.Add(player.PlayerAgent, InsideBoxReward));
    }
    public void Evaluate1v1(PlayerAgent agent)
    {
        if (!ending && agent.Controller.StateMachine.CurrentState is PlayerDeadState)
        {
            ending = true;
            BombermanSettings.Instance.StartCoroutine(Eval(agent));
        }
    }
    public void Evaluate1v1(Arena arena)
    {
        if (!ending && arena.Players.Any(p => p.StateMachine.CurrentState is PlayerDeadState))
        {
            ending = true;
            BombermanSettings.Instance.StartCoroutine(Eval(arena));
        }
    }
    public int Steps;
    private int stepCounter = 0;
    public void MaxSteps(Arena arena)
    {
        if (!ending)
        {
            if (stepCounter >= Steps)
            {
                ending = true;
                if (arena.Players.Any(p => p.StateMachine.CurrentState is PlayerDeadState))
                {
                    BombermanSettings.Instance.StartCoroutine(Eval(arena));
                }
                else
                {
                    BombermanSettings.Instance.StartCoroutine(Draw(arena));
                }
            }
            else
            {
                stepCounter++;
            }
        }
    }
    private IEnumerator Draw(Arena arena)
    {
        PlayerAgent first = arena.Players[0].PlayerAgent;
        PlayerAgent second = arena.Players[1].PlayerAgent;

        first.SetReward(DrawReward);
        second.SetReward(DrawReward);
        first.Done();
        second.Done();

        yield return new WaitForFixedUpdate();
        ResetEnv(arena);
    }
    private IEnumerator Eval(PlayerAgent agent)
    {
        for (int i = 0; i < 5; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        
        PlayerAgent other = agent.Controller.Arena.Players.First(p => p != agent.Controller).PlayerAgent;

        if (other.Controller.StateMachine.CurrentState is PlayerDeadState)
        {
            agent.SetReward(DrawReward);
            other.SetReward(DrawReward);
            agent.Done();
            other.Done();
        }
        else
        {
            agent.SetReward(-1.0f);
            other.SetReward(1.0f);
            agent.Done();
            other.Done();
        }
        yield return new WaitForFixedUpdate();
        ResetEnv(agent.Controller.Arena);
    }
    private IEnumerator Eval(Arena arena)
    {
        for (int i = 0; i < 5; i++)
        {
            yield return new WaitForFixedUpdate();
        }

        PlayerAgent first = arena.Players[0].PlayerAgent;
        PlayerAgent second = arena.Players[1].PlayerAgent;

        if (first.Controller.StateMachine.CurrentState is PlayerDeadState && second.Controller.StateMachine.CurrentState is PlayerDeadState)
        {
            first.SetReward(DrawReward);
            second.SetReward(DrawReward);
            first.Done();
            second.Done();
        }
        else if (first.Controller.StateMachine.CurrentState is PlayerDeadState)
        {
            // Suicide
            if (first.Controller.Team == first.Controller.LastKilledBy.Team)
            {
                first.SetReward(-1.0f);
                second.SetReward(1.0f);
            }
            // Kill
            else
            {
                first.SetReward(-1.0f);
                second.SetReward(1.0f);
            }
            first.Done();
            second.Done();
        } 
        else
        {
            // Suicide
            if (second.Controller.Team == second.Controller.LastKilledBy.Team)
            {
                first.SetReward(1.0f);
                second.SetReward(-1.0f);
            }
            // Kill
            else
            {
                first.SetReward(1.0f);
                second.SetReward(-1.0f);
            }
            first.Done();
            second.Done();
        }
        yield return new WaitForFixedUpdate();
        ResetEnv(arena);
    }
    public void AddRewardUpdate(PlayerAgent player)
    {
        player.AddReward(UpdateReward);
    }
    public void SpawnBomb(BombController bomb)
    {
        bomb.Owner.PlayerAgent.AddReward(SpawnBombReward);
    }
    public void SpawnBombInRange(BombController bomb)
    {
        PlayerAgent first = bomb.Arena.Players[0].PlayerAgent;
        PlayerAgent second = bomb.Arena.Players[1].PlayerAgent;
        float distance = Vector3.Distance(first.transform.position, second.transform.position);
        if (distance > 0.3f && distance < 5.5f)
        {
            bomb.Owner.PlayerAgent.AddReward(SpawnBombReward);
        }
    }
    public void KickBomb(BombController bomb)
    {
        Transform other = bomb.Owner.Arena.Players.First(p => p != bomb.Owner).transform;
        float dot = Vector3.Dot(bomb.Owner.AimVector.normalized, (other.position - bomb.transform.position).normalized);
        bomb.Owner.PlayerAgent.AddReward(KickBombReward);// * dot);
    }
    public void DistanceReward(Arena arena)
    {
        PlayerAgent first = arena.Players[0].PlayerAgent;
        PlayerAgent second = arena.Players[1].PlayerAgent;

        float distance = Vector3.Distance(first.transform.position, second.transform.position);

        if (distance < 3.5f)
        {
            first.AddReward(-InRangeReward * 2f);
            second.AddReward(-InRangeReward * 2f);
        }
        else if (distance < 5.5f)
        {
            first.AddReward(InRangeReward);
            second.AddReward(InRangeReward);
        }
        else
        {
            first.AddReward(-InRangeReward);
            second.AddReward(-InRangeReward);
        }
    }
    public void UpdateWithPenalty(Arena arena)
    {
        arena.Players.ForEach(p => p.PlayerAgent.AddReward(UpdateReward));
    }
    public void AddInsideBoxReward(Arena arena)
    {
        arena.Players.ForEach(player =>
        {
            Vector3 pos = player.transform.position;
            pos.y = 0.0f;
            Collider[] colliders = Physics.OverlapSphere(player.transform.position, 0.5f, BoxLayer);
            if (colliders.Any(coll =>
            {
                Vector3 cpos = coll.transform.position;
                cpos.y = 0.0f;
                return Vector3.Distance(pos, cpos) < 0.5f;
            }))
            {
                player.PlayerAgent.AddReward(InsideBoxReward);
            }
        });
    }
    public void AddOutsideBoundsReward(Arena arena)
    {
        arena.Players.ForEach(player =>
        {
            if (OutsideReward == null)
                OutsideReward = new Dictionary<PlayerAgent, float>();
            if (!OutsideReward.ContainsKey(player.PlayerAgent))
                OutsideReward.Add(player.PlayerAgent, InsideBoxReward);

            Vector3 pos = player.transform.localPosition;
            int size = arena.WorldSize;
            
            if (pos.x < 0.5f || pos.z < 0.5f || pos.x > size - 1.5f || pos.z > size - 1.5f)
            {
                player.PlayerAgent.AddReward(OutsideReward[player.PlayerAgent]);
                OutsideReward[player.PlayerAgent] *= .9f;
            }
        });
    }
}
