using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using GNW2.Input;
using UnityEngine;

namespace GNW2.Player
{
    public class Player : NetworkBehaviour
    {
        private NetworkCharacterController _cc;
        [SerializeField] private float speed = 5f;

        private void Awake()
        {
            _cc = GetComponent<NetworkCharacterController>();
        }

        public override void FixedUpdateNetwork()
        {
            if (GetInput(out NetworkInputData data))
            {
                data.Direction.Normalize();
                _cc.Move(speed *data.Direction * Runner.DeltaTime);
            }
        }
        
        
    }
}
