using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using GNW2.Input;
using GNW2.Projectile;
using GNW2.UI;
using Unity.VisualScripting;
using UnityEngine;

namespace GNW2.Player
{
    public class Player : NetworkBehaviour, ICombat
    {
        [SerializeField] private float speed = 5f;
        [SerializeField] private BulletProjectile bulletPrefab;
        [SerializeField] private float fireRate = 0.1f;
        [Networked] private TickTimer fireDelayTimer { get; set; }
        private Vector3 _bulletSpawnLocation = Vector3.forward * 2;
        private Vector3 _lastKnownInput = Vector3.zero;
        private NetworkCharacterController _cc;
        private ChatUI _chatUI;

        [SerializeField]private Animator _playerAnimator;
        private static readonly int IsMoving = Animator.StringToHash("IsMoving");
        private static readonly int Jump = Animator.StringToHash("Jump");
        private static readonly int IsInAir = Animator.StringToHash("IsInAir");

        private event Action OnButtonPressed;
        public event Action<int> OnTakeDamage;
        private void Awake()
        {
            _cc = GetComponent<NetworkCharacterController>();
            _chatUI = FindObjectOfType<ChatUI>();
            if (_chatUI != null)
            {
                _chatUI.OnMesageSent += RPC_SendMessage;
            }
        }

        private void Update()
        {
            if (_playerAnimator)
            {
                _playerAnimator.SetBool(IsMoving, _cc.Velocity.magnitude > 0);
                _playerAnimator.SetBool(IsInAir, !_cc.Grounded);
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (!GetInput(out NetworkInputData data)) return;
            
            data.Direction.Normalize();
            _cc.Move(speed *data.Direction * Runner.DeltaTime);
            
            if (!HasStateAuthority || !fireDelayTimer.ExpiredOrNotRunning(Runner)) return;
            
            if (data.Direction.sqrMagnitude > 0)
            {
                _bulletSpawnLocation = transform.forward * 2f;
            }

            if (data.buttons.IsSet(NetworkInputData.MOUSEBUTTON0))
            {
                fireDelayTimer = TickTimer.CreateFromSeconds(Runner, fireRate);
                Runner.Spawn(bulletPrefab, transform.position + _bulletSpawnLocation,
                    Quaternion.LookRotation(_bulletSpawnLocation), Object.InputAuthority,
                    (runner, bullet) => { bullet.GetComponent<BulletProjectile>()?.Init(); });
            }

            if (data.buttons.IsSet(NetworkInputData.JUMPBUTTON) && _cc.Grounded)
            {
                _cc.Jump();
            }
        }

        private void OnBulletSpawned(NetworkRunner runner, NetworkObject bullet)
        {
            bullet.GetComponent<BulletProjectile>()?.Init();
        }


        public void TakeDamage(int Damage)
        {
            OnTakeDamage?.Invoke(Damage);
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.Proxies)]
        private void RPC_SendMessage(string message)
        {
            Debug.Log(message);
        }
    }
}
