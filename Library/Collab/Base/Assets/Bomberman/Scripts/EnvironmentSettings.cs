using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Environment/Settings")]
public class EnvironmentSettings : ScriptableObject
{
    [System.Serializable]
    public class PlayerControllerEvent : UnityEvent<PlayerController> { }
    [System.Serializable]
    public class PlayerAgentEvent : UnityEvent<PlayerAgent> { }
    [Header("Game Over")]
    public PlayerControllerEvent OnPlayerDead;
    [Header("Update")]
    public PlayerAgentEvent OnRewardUpdate;
    public float UpdateReward;

    public void Loose1v1(PlayerController player)
    {
        player.PlayerAgent.Loose();
        PlayerController other = player.Arena.Players.First(p => p != player);
        other.PlayerAgent.Win();
        player.Arena.Generate();
    }
    public void AddRewardUpdate(PlayerAgent player)
    {
        player.AddReward(UpdateReward);
    }
}
