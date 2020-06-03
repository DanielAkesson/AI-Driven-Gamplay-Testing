using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AdvancedWallJump
{
    public class AdvancedWallJumpSettings : MonoBehaviour
    {
        [Header("Agent")]
        public LayerMask GroundLayers;
        public float GroundCheckDistance;
        public float MovementSpeed;
        public float FallingMultiplier;
        public float JumpForce;
    }
}

