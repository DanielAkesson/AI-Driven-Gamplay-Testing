using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RollerBall
{
    public class RollerArea : MonoBehaviour
    {
        public GameObject Agent0Prefab;
        public GameObject Agent1Prefab;
        public GameObject Target;

        private RollerAgent agent0;
        private RollerAgent agent1;

        private void Awake()
        {
            CreateAgents();
        }
        private void CreateAgents()
        {
            agent0 = Instantiate(Agent0Prefab, transform).GetComponent<RollerAgent>();
            agent0.Area = this;
            agent0.target = Target.transform;
            agent1 = Instantiate(Agent1Prefab, transform).GetComponent<RollerAgent>();
            agent1.Area = this;
            agent1.target = Target.transform;

            agent0.otherAgent = agent1;
            agent1.otherAgent = agent0;
        }
        public void ResetEnv()
        {
            Destroy(agent0.gameObject);
            Destroy(agent1.gameObject);

            CreateAgents();
        }
    }
}

