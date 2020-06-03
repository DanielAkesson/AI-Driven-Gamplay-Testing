using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateKraft;

public class BombKickState : State
{
    [Header("Kick Properties")]
    public float FlightTime;
    public float FlightHeight;
    public int KickDistance;
    public AnimationCurve FlightPath;
    public LayerMask CollideWith;

    [HideInInspector]
    public Vector3 KickDirection;

    //Private stuff
    private float currentTime;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private BombController _owner;
    private bool hasHitWall = false;
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
        owner.IdleWall.SetActive(false);
        currentTime = 0f;
        Vector3 localPosition = new Vector3(Mathf.RoundToInt(owner.transform.localPosition.x), 0, Mathf.RoundToInt(owner.transform.localPosition.z));
        startPosition = owner.Arena.transform.TransformPoint(localPosition);
        targetPosition = startPosition + KickDirection.normalized * KickDistance;
    }
    public override void StateUpdate()
    {
        //Check if bomb should explode i guess
        owner.CheckIfExplosion();
        //Calculate next position
        currentTime += Arena.TimeStep;
        Move();
        if (currentTime > FlightTime)
            StateMachine.TransitionTo<BombTickState>();
    }
    public bool AllowKick()
    {
        return currentTime / FlightTime > 0.5f;
    }
    private void Move()
    {
        float factor = currentTime / FlightTime;
        Vector3 nextPosition = Vector3.Lerp(startPosition, targetPosition, factor) + Vector3.up * FlightPath.Evaluate(factor) * FlightHeight;

        //If box in way make bomb fall ground
        RaycastHit hit;
        Physics.SphereCast(transform.position, 1f / 3f, (nextPosition - owner.transform.position).normalized, out hit, (nextPosition - owner.transform.position).magnitude, CollideWith);
        Vector3 sphereCenterOnHit = owner.transform.position + (nextPosition - owner.transform.position).normalized * hit.distance;
        Vector3 localPosition = new Vector3(Mathf.RoundToInt(owner.transform.localPosition.x), 0, Mathf.RoundToInt(owner.transform.localPosition.z));
        //If hit stop movment
        if (hit.collider != null)
        {
            targetPosition = owner.Arena.transform.TransformPoint(localPosition);
            startPosition = new Vector3(hit.point.x, hit.point.y, hit.point.z);
            hasHitWall = true;
        }

        //If it lands on top (Does not work well)
        Vector3 currentCell = owner.Arena.transform.TransformPoint(localPosition);
        Collider[] boxes = Physics.OverlapBox(currentCell, Vector3.one / 4f, Quaternion.identity, CollideWith);
        if (boxes.Length > 0 && hasHitWall)
        {
            currentCell -= KickDirection * 1.2f;
            Vector3 newPos = owner.Arena.transform.InverseTransformPoint(currentCell);
            Vector3 rounded = new Vector3(Mathf.RoundToInt(newPos.x), Mathf.RoundToInt(newPos.y), Mathf.RoundToInt(newPos.z));
            targetPosition = owner.Arena.transform.TransformPoint(rounded);
        }

        //Move
        owner.transform.position = nextPosition;
    }
    public override void Exit()
    {
        Vector3 localPosition = new Vector3(Mathf.RoundToInt(owner.transform.localPosition.x), 0, Mathf.RoundToInt(owner.transform.localPosition.z));
        owner.transform.position = owner.Arena.transform.TransformPoint(localPosition);
    }
}
