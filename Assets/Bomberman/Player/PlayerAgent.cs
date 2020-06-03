using MLAgents;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerAgent : Agent
{
    public PlayerController Controller;
    public BehaviorParameters Parameters;
    [HideInInspector] public bool IsDone;


    private bool KickButton;
    private bool DropButton;

    public override void InitializeAgent()
    {
        Controller.StateMachine.Initialize(Controller);
        IsDone = false;
        Controller.CurrentBombAmount = Controller.BombCapacity;
    }
    public override void AgentReset()
    {
        IsDone = false;
        Controller.CurrentBombAmount = Controller.BombCapacity;
    }
    public override void CollectObservations()
    {
        // ------------------------- SELF -------------------------
        // Vector size = 486
        float[] nearbyMap = Controller.Arena.GetNearby(Controller, 4, Arena.Mapping.Enemy, Arena.Mapping.Bomb, Arena.Mapping.Wall, Arena.Mapping.Box, Arena.Mapping.Unwalkable, Arena.Mapping.Powerup);
        // Vector size = 120
        float[] compass = Controller.Arena.GetCompass(Controller, 4, Arena.Mapping.Enemy, Arena.Mapping.Bomb, Arena.Mapping.Powerup);
        foreach (float f in nearbyMap)
        {
            AddVectorObs(f);
        }
        foreach (float f in compass)
        {
            AddVectorObs(f);
        }

        // Position within cell, Vector size = 2
        AddVectorObs((transform.localPosition.x - Mathf.Round(transform.localPosition.x)) * 2f);
        AddVectorObs((transform.localPosition.z - Mathf.Round(transform.localPosition.z)) * 2f);

        // Bomb
        // Vector size = 2
        AddVectorObs((float)Controller.CurrentBombAmount / (float)Controller.BombCapacity);
        AddVectorObs(Controller.CurrentBombCD / Controller.BombDropCooldown);

        // PowerUp
        // Vector size = 1
        AddVectorObs(Controller.PoweredUpTime > 0f);

        // ------------------------- ENEMY -------------------------
        PlayerController enemyController = Controller.Arena.Players.First(p => p != Controller);

        // Relative position enemy
        Vector3 relativeVector = enemyController.transform.position - Controller.transform.position;
        // Kick distance = 4, Explode radius = 3
        AddVectorObs(Mathf.Clamp01(relativeVector.magnitude / 7f));
        relativeVector.Normalize();
        AddVectorObs(relativeVector.x);
        AddVectorObs(relativeVector.z);
        // Vector size = 196
        /*float[] enemyNearbyMap = enemyController.Arena.GetNearby(enemyController, 3, Arena.Mapping.Enemy, Arena.Mapping.Bomb, Arena.Mapping.Wall, Arena.Mapping.Box);
        foreach (float f in enemyNearbyMap)
        {
            AddVectorObs(f);
        }*/
    }
    public override void AgentAction(float[] action)
    {
        /* Discrete
        Vector3 vertical = Vector3.zero;
        Vector3 horizontal = Vector3.zero;
        switch ((int)action[0])
        {
            case 1:
                vertical = Vector3.forward;
                break;
            case 2:
                vertical = Vector3.back;
                break;
        }

        switch ((int)action[1])
        {
            case 1:
                horizontal = Vector3.right;
                break;
            case 2:
                horizontal = Vector3.left;
                break;
        }
        Controller.InputVector = (vertical + horizontal).normalized;
        Controller.KickButton = (int)action[2] == 1;
        Controller.DropBombButton = (int)action[3] == 1;
        */
        // Continuous
        Controller.MoveVector = (Vector3.forward *
                                  Mathf.Clamp(action[0], -1f, 1f) +
                                  Vector3.right *
                                  Mathf.Clamp(action[1], -1f, 1f)).normalized;

        Controller.AimVector = (Vector3.forward *
                                  Mathf.Clamp(action[2], -1f, 1f) +
                                  Vector3.right *
                                  Mathf.Clamp(action[3], -1f, 1f)).normalized;

        bool kickPress = action[4] > 0f;
        bool dropPress = action[5] > 0f;
        Controller.KickButton = (!KickButton) && kickPress;
        Controller.DropBombButton = (!DropButton) && dropPress;
        KickButton = kickPress;
        DropButton = dropPress;

        if (Controller.ControlledByHuman)
        {
            Controller.PullController();
        }
        Controller.Step();
        Controller.StateMachine.Update();
    }
    public override float[] Heuristic()
    {
        Controller.ControlledByHuman = true;
        return new float[6];
    }
}
