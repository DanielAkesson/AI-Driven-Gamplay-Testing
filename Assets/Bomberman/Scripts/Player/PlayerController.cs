using UnityEngine;
using StateKraft;
public class PlayerController : MonoBehaviour
{
    public int Team => PlayerAgent.Parameters.m_TeamID;
    [Header("Settings")]
    public Arena Arena;
    public Renderer PlayerRender;
    public int BombCapacity = 3;
    [HideInInspector]
    public int CurrentBombAmount = 3;
    [Header("Controller Properties")]
    public PlayerAgent PlayerAgent;
    public StateMachine StateMachine;
    public Vector3 Velocity;
    public GameObject Bomb;
    public float BombDropCooldown;
    [HideInInspector]
    public float CurrentBombCD;
    [Header("Input methods and Vectors")]
    public bool ControlledByHuman = false;
    //Input vectors
    public Vector3 MoveVector;
    public Vector3 AimVector;
    public bool DropBombButton;
    public bool KickButton;

    private static int SavedByLine;
    private static int NotSavedByLine;
    private static int PowerDoNothingDeath;
    private static int NoPowerDeath;

    [HideInInspector] public float PoweredUpTime;
    private int PowerUpStacking;
    public BombController LastKilledBy { get; private set; }

    public void PullController()
    {
        MoveVector = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        AimVector = new Vector3(Input.GetAxisRaw("AimHorizontal"), 0, Input.GetAxisRaw("AimVertical"));
        Debug.DrawLine(transform.position, transform.position + AimVector);
        DropBombButton = Input.GetButton("Bomb");
        KickButton = Input.GetButton("Kick");
    }
    public void Step()
    {
        if (PoweredUpTime >= 0f && (PowerUpStacking < BombermanSettings.GetEvno(Arena).PowerStackingNumber && BombermanSettings.GetEvno(Arena).PowerStacking))
            PoweredUpTime -= Arena.TimeStep;
        if (PoweredUpTime <= 0f)
            PowerUpStacking = 0;

        if (StandingOnLine(transform.localPosition.x) || StandingOnLine(transform.localPosition.z))
            BombermanSettings.onLine++;
        else
            BombermanSettings.offLine++;

        if (Input.GetKeyDown(KeyCode.R))
            Debug.Log("On line: " + ((float)BombermanSettings.onLine / (float)(BombermanSettings.onLine + BombermanSettings.offLine)));
    }
    public void PowerUp(float PowerUpTime)
    {
        if (PoweredUpTime >= 0f)
            PowerUpStacking++;
        PoweredUpTime = PowerUpTime;
    }
    public void Hit(BombController bomb)
    {
        if (PoweredUpTime > 0 && !BombermanSettings.GetEvno(Arena).PowerDoNothing)
            return;

        if (PoweredUpTime > 0)
        {
            PowerDoNothingDeath++;
        }
        else
        {
            NoPowerDeath++;
        }
        //5 Debug.Log(string.Format("Had power: {0}, total: {1}", PowerDoNothingDeath, PowerDoNothingDeath + NoPowerDeath));


        if ((StandingOnLine(transform.localPosition.x) || StandingOnLine(transform.localPosition.z)) && BombermanSettings.GetEvno(Arena).CellLineProtection)
        {
            SavedByLine++;
            //Debug.Log(string.Format("Saved: {0}, Total: {1}", SavedByLine, SavedByLine + NotSavedByLine));
            return;
        }
        else
        {
            NotSavedByLine++;
            //Debug.Log(string.Format("Saved: {0}, Total: {1}", SavedByLine, SavedByLine + NotSavedByLine));
        }

        LastKilledBy = bomb;
        StateMachine.TransitionTo<PlayerDeadState>();
    }

    public bool StandingOnLine(float d)
    {
        float x = Mathf.Abs(d - Mathf.Floor(d) - 0.5f) * 2f;
        return x < BombermanSettings.GetEvno(Arena).CellLineProtectionSize;
    }
}
