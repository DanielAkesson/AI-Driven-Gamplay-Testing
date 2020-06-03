using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateKraft;

public class BombExplodeState : State
{

    public LayerMask BombsHitLayer;
    public LayerMask BombsBlockedLayer;
    public float TimeInbetweenExplosions = 0.5f;
    public bool ExplodeDiagonal = false;
    public GameObject ExplosionGrapic;
    public bool ShowDeBugExplosion = false;


    private bool[][] blocked;
    private BombController _owner;
    private BombController owner
    {
        get
        {
            if (_owner == null)
                _owner = gameObject.GetComponent<BombController>();
            return _owner;
        }
    }
    private int explosionIndex;
    private float currentTime;

    public override void Enter()
    {
        owner.IdleWall.SetActive(false);
        blocked = new bool[3][];
        for (int i = 0; i < 3; i++)
            blocked[i] = new bool[3];

        transform.localPosition = new Vector3(Mathf.RoundToInt(transform.localPosition.x), Mathf.RoundToInt(transform.localPosition.y), Mathf.RoundToInt(transform.localPosition.z));
    }

    public override void StateUpdate()
    {
        currentTime += Arena.TimeStep;
        if(currentTime > TimeInbetweenExplosions)
        {
            Explode(explosionIndex, ExplodeDiagonal);
            explosionIndex++;
            currentTime = 0;
        }
    }

    private void Explode(int explotionLayer, bool explodeDiagonally)
    {
        if (explotionLayer > owner.Power)
        {
            owner.Arena.DestroyGameObject(gameObject);
            if(owner.Placer != null)
                owner.Placer.CurrentBombAmount++;
            return;
        }

        if (explotionLayer == 0)
        {
            ExplodeTile(Mathf.RoundToInt(transform.localPosition.x), Mathf.RoundToInt(transform.localPosition.z));
            return;
        }

        //Explode in all directions at layaer
        for(int x = -1; x < 2; x ++)
        {
            for (int z = -1; z < 2; z ++)
            {
                if (x == 0 && z == 0)
                    continue; // No explosions when zero vector

                if (x != 0 && z != 0 && !explodeDiagonally)
                    continue; // avoid diagonal explosions

                if (blocked[x+1][z+1])
                    continue; // Explosion has been blocked in this direction

                Vector3 cellLoc = new Vector3(x * explotionLayer, 0, z * explotionLayer) + transform.localPosition;
                blocked[x+1][z+1] = ExplodeTile(Mathf.RoundToInt(cellLoc.x), Mathf.RoundToInt(cellLoc.z));
            }
        }
    }

    private bool ExplodeTile(int xTile, int zTile)
    {
        Vector3 ExpolsionCenterWorldSpace =  owner.Arena.transform.TransformPoint(new Vector3(xTile, 0, zTile));
        Collider[] hits = Physics.OverlapBox(ExpolsionCenterWorldSpace, Vector3.one / 3f, Quaternion.identity, BombsHitLayer);
        Collider[] blocks = Physics.OverlapBox(ExpolsionCenterWorldSpace, Vector3.one / 3f, Quaternion.identity, BombsBlockedLayer);

        //Is blocked
        if(blocks.Length > 0)
            return true;

        //Do come visual gargabe
        if(ShowDeBugExplosion)
        {
            Debug.DrawLine(ExpolsionCenterWorldSpace + new Vector3(0.5f, 0, 0.5f), ExpolsionCenterWorldSpace + new Vector3(-0.5f, 0, -0.5f), Color.red, 0.2f);
            Debug.DrawLine(ExpolsionCenterWorldSpace + new Vector3(0.5f, 0, -0.5f), ExpolsionCenterWorldSpace + new Vector3(-0.5f, 0, 0.5f), Color.red, 0.2f);
        }
        else
            Instantiate(ExplosionGrapic, ExpolsionCenterWorldSpace, Quaternion.identity, null);

        //Call hits on players and bomb in square!
        for (int i = 0; i < hits.Length; i++)
        {
            //Skip if object is not in right cell
            bool sameX = Mathf.RoundToInt(hits[i].gameObject.transform.localPosition.x) == xTile;
            bool sameZ = Mathf.RoundToInt(hits[i].gameObject.transform.localPosition.z) == zTile;
            if (!sameX || !sameZ)
                continue;

            PlayerController player = hits[i].GetComponentInParent<PlayerController>();
            if (player != null)
                player.Hit(owner);

            BombController bomb = hits[i].GetComponentInParent<BombController>();
            if (bomb != null)
                bomb.Hit(owner);

            BoxController box = hits[i].GetComponentInParent<BoxController>();
            if (box != null)
            {
                box.Hit(owner);
                return true;
            }
        }

        return false;
    }

    public override void Exit()
    {
    }
}
