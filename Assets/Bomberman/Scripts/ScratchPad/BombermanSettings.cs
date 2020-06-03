using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class BombermanSettings : MonoBehaviour
{
    public static BombermanSettings Instance;
    [SerializeField] private EnvironmentSettings Settings;
    private Dictionary<Arena, EnvironmentSettings> ArenaSettings = new Dictionary<Arena, EnvironmentSettings>();

    public static void OnPlayerDead(PlayerController player) => Instance.ArenaSettings[player.Arena].OnPlayerDead.Invoke(player);
    public static void OnUpdate(PlayerAgent agent) => Instance.ArenaSettings[agent.Controller.Arena].OnAgentUpdate.Invoke(agent);
    public static void OnUpdate(Arena arena) => Instance.ArenaSettings[arena].OnArenaUpdate.Invoke(arena);
    public static void OnBombSpawn(BombController bomb) => Instance.ArenaSettings[bomb.Arena].OnBombSpawn.Invoke(bomb);
    public static void OnBombKick(BombController bomb) => Instance.ArenaSettings[bomb.Arena].OnBombKick.Invoke(bomb);

    public static int onLine;
    public static int offLine;

    public static EnvironmentSettings GetEvno(Arena a)
    {
        return Instance.ArenaSettings.ContainsKey(a) ? Instance.ArenaSettings[a] : null;
    }
    private void Awake()
    {
        if (Instance != null)
            Debug.LogError("Multiple Settings");
        Instance = this;

        Arena[] arenas = FindObjectsOfType<Arena>();
        foreach (Arena arena in arenas)
        {
            ArenaSettings.Add(arena, Instantiate(Settings));
        }
    }
}