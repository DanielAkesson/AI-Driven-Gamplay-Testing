using Netkraft;
using Netkraft.Messaging;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Messenging
{
    public struct JoinMessage : IReliableMessage, IAcknowledged
    {
        public int seed;

        public void OnAcknowledgment(ClientConnection context)
        {
            Debug.Log("Player Ready");
        }

        public void OnReceive(ClientConnection context)
        {
            DummyArena._instance.Generate(seed);
            Client.GO = true;
            Debug.Log("Server accepts you into the fold");
        }
    }
    public struct PlayerInput : IUnreliableMessage
    {
        public Vector3 Move;
        public Vector3 Aim;
        public bool Kick;
        public bool Bomb;

        public void OnReceive(ClientConnection context)
        {
            PlayerController WhosThatPlayerExclamation = HostServer._instance.Player1 == context ? Arena._instance.Players[0] : Arena._instance.Players[1];
            WhosThatPlayerExclamation.MoveVector = Move;
            WhosThatPlayerExclamation.AimVector = Aim;
            WhosThatPlayerExclamation.KickButton = Kick;
            WhosThatPlayerExclamation.DropBombButton = Bomb;
        }
    }
    public struct WorldState : IUnreliableMessage
    {
        public PlayerWorldState Player1;
        public PlayerWorldState Player2;
        public BombWorldState[] Bombs;
        public ExplosionWorldState[] Explosions;

        public void OnReceive(ClientConnection context)
        {
            LerpDummy(DummyArena._instance.PlayerDummies[0], Player1.Position);
            LerpDummy(DummyArena._instance.PlayerDummies[1], Player2.Position);

            //Bombs
            for(int i=0; i < DummyArena._instance.BombDummies.Count; i++)
            {
                BombDummy b = DummyArena._instance.BombDummies[i];
                if (Bombs.Length < i)
                    b.gameObject.SetActive(false);
                else
                {
                    b.gameObject.SetActive(true);
                    b.OcsolateColor(Bombs[i].Fuse);
                    LerpDummy(b.gameObject, Bombs[i].Position);
                }
            }

            //Explosions
            for (int i = 0; i < DummyArena._instance.ExplosionDummies.Count; i++)
            {
                DummyExplosion e = DummyArena._instance.ExplosionDummies[i];
                if (Explosions.Length < i)
                    e.gameObject.SetActive(false);
                else
                {
                    e.gameObject.SetActive(true);
                    e.SetFade(Explosions[i].Life);
                    LerpDummy(e.gameObject, Explosions[i].Position);
                }
            }

            //Pickups

        }
    }
    public struct PlayerWorldState : IWritable
    {
        public Vector3 Position;
        public int BombCount;
    }
    public struct BombWorldState : IWritable
    {
        public Vector3 Position;
        public float Fuse;
    }
    public struct ExplosionWorldState : IWritable
    {
        public Vector3 Position;
        public float Life;
    }

    private static void LerpDummy(GameObject dummy, Vector3 position)
    {
        dummy.transform.position = position;
    }
}
