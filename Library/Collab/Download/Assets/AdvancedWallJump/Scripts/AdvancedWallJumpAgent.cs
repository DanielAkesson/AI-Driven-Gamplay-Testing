using MLAgents;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AdvancedWallJump
{
    public class AdvancedWallJumpAgent : Agent
    {
        [Header("Environment")]
        public List<GameObject> Walls;
        public GameObject SmallBlock;
        public GameObject LargeBlock;
        public GameObject[] BlockSpawns;
        public GameObject[] AgentSpawns;

        private float smallBlockY;
        private float largeBlockY;

        private new Rigidbody rigidbody;
        private AdvancedWallJumpSettings settings;

        public LayerMask GroundLayers => settings.GroundLayers;
        public float GroundCheckDistance => settings.GroundCheckDistance;
        public float MovementSpeed => settings.MovementSpeed;
        public float FallingMultiplier => settings.FallingMultiplier;
        public float JumpForce => settings.JumpForce;
        public bool IsGrounded() =>
            Physics.BoxCast(transform.position, transform.localScale / 4f, Vector3.down, transform.rotation, GroundCheckDistance, GroundLayers);

        public override void InitializeAgent()
        {
            settings = FindObjectOfType<AdvancedWallJumpSettings>();
            rigidbody = GetComponent<Rigidbody>();
            smallBlockY = SmallBlock.transform.position.y;
            largeBlockY = LargeBlock.transform.position.y;
        }
        public override void AgentReset()
        {
            AgentSpawns = AgentSpawns.OrderBy(a => Random.value).ToArray();
            transform.position = AgentSpawns[0].transform.position;
            BlockSpawns = BlockSpawns.OrderBy(b => Random.value).ToArray();
            Vector3 position = BlockSpawns[0].transform.position;
            position.y = smallBlockY;
            SmallBlock.transform.position = position;
            position = BlockSpawns[1].transform.position;
            position.y = largeBlockY;
            LargeBlock.transform.position = position;
            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
            Rigidbody small = SmallBlock.GetComponent<Rigidbody>();
            Rigidbody large = LargeBlock.GetComponent<Rigidbody>();
            small.angularVelocity = Vector3.zero;
            small.velocity = Vector3.zero;
            large.angularVelocity = Vector3.zero;
            large.velocity = Vector3.zero;

            ConfigureCurriculum();
        }
        public override void CollectObservations()
        {
            AddVectorObs(transform.localPosition / 20f);
            AddVectorObs(IsGrounded() ? 1 : 0);
        }
        public override void AgentAction(float[] vectorAction)
        {
            MoveAgent(vectorAction);
            CheckBricked();
        }
        private void MoveAgent(float[] action)
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
            if ((int)action[2] == 1)
            {
                Jump();
            }

            rigidbody.AddForce((vertical + horizontal) * MovementSpeed, ForceMode.VelocityChange);
            if (!IsGrounded() && Vector3.Dot(rigidbody.velocity, Vector3.down) > -0.1f)
            {
                rigidbody.AddForce(Vector3.down * FallingMultiplier, ForceMode.Acceleration);
            }

            if (rigidbody.velocity.sqrMagnitude > 20f)
            {
                rigidbody.velocity *= 0.95f;
            }
        }
        private void Jump()
        {
            if (IsGrounded())
            {
                transform.position += Vector3.up * GroundCheckDistance;
                rigidbody.AddForce(Vector3.up * JumpForce, ForceMode.VelocityChange);
            }
        }
        private void CheckBricked()
        {
            if (!Physics.Raycast(transform.position, Vector3.down, 20)
            || !Physics.Raycast(SmallBlock.transform.position, Vector3.down, 20)
            || !Physics.Raycast(LargeBlock.transform.position, Vector3.down, 20))
            {
                SetReward(-1f);
                Done();
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
        private void OnTriggerStay(Collider coll)
        {
            if (coll.gameObject.CompareTag("goal") && IsGrounded())
            {
                SetReward(1f);
                Done();
            }
        }
        private void ConfigureCurriculum()
        {
            float min = Academy.Instance.
                                FloatProperties.
                                GetPropertyWithDefault("wall_min_height", 0);
            float max = Academy.Instance.
                                FloatProperties.
                                GetPropertyWithDefault("wall_max_height", 13);

            Walls.ForEach(w =>
            {
                Vector3 scale = w.transform.localScale;
                scale.y = Random.Range(min, max);
                w.transform.localScale = scale;
            });
        }
    }
}

