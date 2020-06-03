using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public LayerMask PlayerLayer;
    public float PowerUpTime;

    // Data Collection
    private static int Spawned;
    private static int Taken;
    private static int DoubleTake;

    private void Start()
    {
        Spawned++;
    }

    private void FixedUpdate()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 0.2f, PlayerLayer);
        if (colliders.Length > 0)
        {
            PlayerController owner = colliders[0].GetComponent<PlayerController>();

            Taken++;
            if (owner.PoweredUpTime > 0f)
            {
                DoubleTake++;
            }
            //Debug.Log(string.Format("Spawned: {0}, Taken: {1}, DoubleUp: {2}", Spawned, Taken, DoubleTake));

            owner.PowerUp(PowerUpTime);
            owner.Arena.DestroyGameObject(this.gameObject);
            //owner.PlayerAgent.AddReward(0.05f);
        }
    }
}
