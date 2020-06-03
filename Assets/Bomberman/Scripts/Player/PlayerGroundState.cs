using StateKraft;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundState : State
{
    public float accerationConstant;
    public float FrictionConstant;
    public float MaxSpeed;
    public float KickCooldownTime;
    public float DropBombCooldownTime;
    public LayerMask CollideLayer;
    public LayerMask KickLayer;
    private PlayerController _owner;
    public float IgnoreCollisionTime = 0f;
    private PlayerController owner
    {
        get
        {
            if(_owner == null)
                _owner = gameObject.GetComponent<PlayerController>();
            return _owner;
        }
    }

    public override void StateUpdate()
    {
        owner.CurrentBombCD += Arena.TimeStep;
        owner.CurrentBombCD = Mathf.Clamp(owner.CurrentBombCD, 0, owner.BombDropCooldown);

        if (owner.KickButton)
            AttemptKick();

        if (owner.DropBombButton && owner.CurrentBombCD >= owner.BombDropCooldown)
            DropBomb();
        MoveUpdate();
    }

    private void AttemptKick()
    {
        if (owner.AimVector.magnitude < .5)
            return;

        Collider[] bombs = Physics.OverlapSphere(owner.transform.position + owner.AimVector.normalized/2f, 1f, KickLayer);
        if (bombs.Length <= 0)
            return;

        bombs[0].GetComponentInParent<BombController>().KickBomb(owner);
        StateMachine.GetState<PlayerCooldownState>().Setup(this, KickCooldownTime);
        StateMachine.TransitionTo<PlayerCooldownState>();
    }
    private void MoveUpdate()
    {
        //Move
        owner.Velocity += owner.MoveVector.normalized * accerationConstant * Arena.TimeStep;

        //Friction
        if (owner.Velocity.magnitude < FrictionConstant * Arena.TimeStep)
            owner.Velocity = Vector3.zero;
        else
            owner.Velocity -= owner.Velocity.normalized * FrictionConstant * Arena.TimeStep;

        //Limit to MaxSpeed
        if (owner.Velocity.magnitude > MaxSpeed)
            owner.Velocity = owner.Velocity.normalized * MaxSpeed;

        if (IgnoreCollisionTime <= 0)
            PhysicsHelper.PreventCollision(cast, ref owner.Velocity, transform, Arena.TimeStep, 0.1f);
        else
            IgnoreCollisionTime -= Arena.TimeStep;
        //Move char
        transform.position += owner.Velocity * Arena.TimeStep;
    }

    private void DropBomb()
    {
        owner.CurrentBombCD = 0;
        Vector3 cellPos = new Vector3(Mathf.RoundToInt(transform.localPosition.x), Mathf.RoundToInt(transform.localPosition.y), Mathf.RoundToInt(transform.localPosition.z));
        Vector3 cellPosWorld = owner.Arena.transform.TransformPoint(cellPos);
        if (!Physics.CheckBox(cellPos, Vector3.one / 2.1f, Quaternion.identity, KickLayer) && owner.CurrentBombAmount > 0)
        {
            if (BombermanSettings.GetEvno(_owner.Arena).PlayerColliderIgnoredOnBombDrop)
                IgnoreCollisionTime = BombermanSettings.GetEvno(_owner.Arena).PlayerColliderIgnoredOnBombDropTime;

            BombController bomb = Instantiate(owner.Bomb, cellPosWorld, Quaternion.identity, owner.Arena.transform).GetComponent<BombController>();
            bomb.Arena = owner.Arena;
            owner.Arena.RegisterGameObject(bomb.gameObject);
            owner.CurrentBombAmount--;
            bomb.Placer = owner;
            bomb.Owner = owner;
            bomb.Team = owner.Team;
            StateMachine.GetState<PlayerCooldownState>().Setup(this, DropBombCooldownTime);
            StateMachine.TransitionTo<PlayerCooldownState>();
            BombermanSettings.OnBombSpawn(bomb);
        }
    }

    private RaycastHit cast()
    {
        RaycastHit hit;
        Physics.BoxCast(transform.position, Vector3.one/6f, owner.Velocity.normalized, out hit, Quaternion.identity, float.MaxValue, CollideLayer);
        return hit;
    }
}
