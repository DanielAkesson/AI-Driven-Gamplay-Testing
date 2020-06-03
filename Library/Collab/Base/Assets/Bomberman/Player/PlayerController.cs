using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateKraft;
public class PlayerController : MonoBehaviour
{
    public int Team { get { return PlayerAgent.Parameters.m_TeamID; } set { PlayerAgent.Parameters.m_TeamID = value; } }
    [Header("Settings")]
    public Arena Arena;
    public Renderer PlayerRender;
    [Header("Controller Properties")]
    public PlayerAgent PlayerAgent;
    public StateMachine StateMachine;
    public Vector3 Velocity;
    public GameObject Bomb;

    [Header("Input methods and Vectors")]
    public bool ControlledByHuman = false;
    //Input vectors
    public Vector3 InputVector;
    public bool DropBombButton;
    public bool KickButton;

    private void Start()
    {
        StateMachine.Initialize(this);
        Debug.Log("ID: " + Team);
    }

    public void PullController()
    {
        InputVector = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        DropBombButton = Input.GetButton("Bomb");
        KickButton = Input.GetButton("Kick");
    }

    public void Hit(BombController bomb)
    {
        StateMachine.TransitionTo<PlayerDeadState>();
    }
}
