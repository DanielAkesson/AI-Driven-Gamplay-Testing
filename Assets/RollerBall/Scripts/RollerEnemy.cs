using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RollerBall
{
    public class RollerEnemy : MonoBehaviour
    {
        public Transform target;
        public float speed;
        private void FixedUpdate()
        {
            transform.position += (target.position - transform.position).normalized * speed * Time.deltaTime;
            if (Vector3.Distance(target.position, transform.position) < 1f)
            {
                RollerAgent agent = target.GetComponent<RollerAgent>();
                agent.SetReward(-1f);
                agent.Done();
            }
        }
    }
}

