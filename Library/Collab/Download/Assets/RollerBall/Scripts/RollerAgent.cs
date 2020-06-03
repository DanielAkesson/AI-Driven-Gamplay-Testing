using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

namespace RollerBall
{
    public class RollerAgent : Agent
    {
        public Transform target;
        public GameObject enemyObject;
        public float speed;
        private new Rigidbody rigidbody;
        public RollerAgent otherAgent;

        public RollerArea Area { get; set; }

        private void Start()
        {
            rigidbody = GetComponent<Rigidbody>();
        }
        public override void AgentReset()
        {
            if (transform.position.y < 0)
            {
                rigidbody.angularVelocity = Vector3.zero;
                rigidbody.velocity = Vector3.zero;
                transform.localPosition = new Vector3(0, 0.5f, 0);
            }
            target.localPosition = new Vector3(Random.value * 8 - 4,
                                          0.5f,
                                          Random.value * 8 - 4);
            /*
                        float enemy = Academy.Instance.
                                            FloatProperties.
                                            GetPropertyWithDefault("enemy_difficulty", 0);

                        if (enemy > 0.5f)
                        {
                            enemyObject.SetActive(true);
                            enemyObject.transform.localPosition = new Vector3(Random.value * 8 - 4,
                                                      0.5f,
                                                      Random.value * 8 - 4);
                            enemyObject.GetComponent<RollerEnemy>().speed = enemy;
                        }
                        */
                        /*
            enemyObject.SetActive(true);
            enemyObject.transform.localPosition = new Vector3(Random.value * 8 - 4,
                                      0.5f,
                                      Random.value * 8 - 4);
            enemyObject.GetComponent<RollerEnemy>().speed = 2f;
            */
        }
        public override void CollectObservations()
        {
            AddVectorObs(target.localPosition / 5f);
            AddVectorObs(transform.localPosition / 5f);
            AddVectorObs(otherAgent.transform.localPosition / 5f);
            //AddVectorObs(enemyObject.activeSelf);
            //AddVectorObs(enemyObject.activeSelf ? enemyObject.transform.localPosition / 5f : Vector3.zero);

            AddVectorObs((float)System.Math.Tanh(rigidbody.velocity.x / 5f));
            AddVectorObs((float)System.Math.Tanh(rigidbody.velocity.z / 5f));

            AddVectorObs((float)System.Math.Tanh(otherAgent.rigidbody.velocity.x / 5f));
            AddVectorObs((float)System.Math.Tanh(otherAgent.rigidbody.velocity.z / 5f));

        }
        public override void AgentAction(float[] vectorAction)
        {
            Vector3 controlSingnal = Vector3.zero;
            controlSingnal.x = vectorAction[0];
            controlSingnal.z = vectorAction[1];
            rigidbody.AddForce(controlSingnal * speed);

            float distanceToTarget = Vector3.Distance(transform.position, target.position);

            if (distanceToTarget < 1.42f)
            {
                SetReward(1.0f);
                Done();
                otherAgent.SetReward(-1.0f);
                otherAgent.Done();
            }
            if (transform.position.y < 0)
            {
                SetReward(-1f);
                Done();
                otherAgent.SetReward(1.0f);
                otherAgent.Done();
            }
        }
        public override float[] Heuristic()
        {
            var action = new float[2];
            action[0] = Input.GetAxis("Horizontal");
            action[1] = Input.GetAxis("Vertical");
            return action;
        }
    }
}