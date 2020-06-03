using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bomberman
{
    public class BombermanArea : MonoBehaviour
    {
        public Vector2 Dimensions;
        public List<BombermanAgent> agents;
        public GameObject PickupPrefab;

        public Vector3 RandomPosition => new Vector3(Random.Range(-HalfX, HalfX), 0.5f, Random.Range(-HalfY, HalfY));
        private float HalfX => Dimensions.x / 2f;
        private float HalfY => Dimensions.y / 2f;

        private GameObject activePickup;

        private void Start()
        {
            agents.ForEach(a => a.area = this);
            activePickup = Instantiate(PickupPrefab, transform.position + RandomPosition, Quaternion.identity);
            Debug.Log("Start");
        }
        private void Update()
        {
            List<BombermanAgent> activeAgents = agents.Where(a => a.gameObject.activeSelf).ToList();
            if (activeAgents.Count > 1)
                return;

            if (activeAgents.Count == 1)
            {
                activeAgents[0].SetReward(1f);
            }

            agents.ForEach(a => 
            {
                a.gameObject.SetActive(true);
            });

        }
        public void CollectPickup(GameObject pickup, BombermanAgent agent)
        {
            agent.hasBomb = true;
            pickup.transform.position = transform.position + RandomPosition;
            agents.ForEach(a =>
            {
                if (a == agent)
                {
                    agent.SetReward(1f);
                } else
                {
                    a.SetReward(-1);
                }
                a.gameObject.SetActive(true);
                a.Done();
            });
        }
        public void Win(BombermanAgent agent)
        {
            agents.ForEach(a =>
            {
                if (a == agent)
                    a.SetReward(1.0f);
                else
                    a.SetReward(-1.0f / (agents.Count - 1));

                a.Done();
            });
        }
    }
}