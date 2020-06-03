using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bomberman
{
    public class BombLogic : MonoBehaviour
    {
        public float BombTime;
        public float ExplosionRadius;
        public LayerMask AgentLayer;
        [HideInInspector] public BombermanAgent Owner;
        private float ExpireTime;

        private void Awake()
        {
            ExpireTime = Time.time + BombTime;
        }
        private void Update()
        {
            if (Time.time >= ExpireTime)
            {
                Collider[] colliders = Physics.OverlapSphere(transform.position, ExplosionRadius, AgentLayer);
                foreach (Collider coll in colliders)
                {
                    BombermanAgent agent = coll.GetComponent<BombermanAgent>();
                    if (agent != Owner)
                    {
                        //Owner.SetReward(1f);
                    }
                    //agent.SetReward(-1f);
                    agent.Done();
                    agent.gameObject.SetActive(false);
                }
                Destroy(gameObject);
            }
        }
    }

}