using MLAgents;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bomberman
{
    public class BombermanAgent : Agent
    {
        public float movementSpeed;
        public BombLogic BombPrefab;
        
        private new Rigidbody rigidbody;
        [HideInInspector] public BombermanArea area;

        [HideInInspector] public bool hasBomb;

        public override void InitializeAgent()
        {
            rigidbody = GetComponent<Rigidbody>();
        }
        public override void AgentReset()
        {
            rigidbody.velocity = Vector3.zero;
            transform.localPosition = area.RandomPosition;
            hasBomb = true;
        }
        public override void CollectObservations()
        {
            AddVectorObs(rigidbody.velocity.x);
            AddVectorObs(rigidbody.velocity.z);
            AddVectorObs(hasBomb);
        }
        public void MoveAgent(float[] action)
        {
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

            rigidbody.AddForce((vertical + horizontal) * movementSpeed, ForceMode.VelocityChange);
            if (rigidbody.velocity.sqrMagnitude > 25f)
            {
                rigidbody.velocity *= 0.95f;
            }
        }
        public void UseAbility()
        {
            if (hasBomb)
            {
                hasBomb = false;
                BombLogic bomb = Instantiate(BombPrefab, transform.position, transform.rotation);
                bomb.Owner = this;
            }
        }
        public void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("pickup"))
            {
                area.CollectPickup(collision.gameObject, this);
            }
        }
        public override void AgentAction(float[] vectorAction)
        {
            MoveAgent(vectorAction);
            if ((int)vectorAction[2] == 1) 
            {
                UseAbility();
            }
        }
        public override float[] Heuristic()
        {
            var action = new float[3];
            if (Input.GetKey(KeyCode.W))
            {
                action[0] = 1f;
            }
            if (Input.GetKey(KeyCode.S))
            {
                action[0] = 2f;
            }
            if (Input.GetKey(KeyCode.D))
            {
                action[1] = 1f;
            }
            if (Input.GetKey(KeyCode.A))
            {
                action[1] = 2f;
            }
            action[2] = Input.GetKey(KeyCode.Space) ? 1.0f : 0.0f;
            return action;
        }
    }
}