using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Fusion;
using GNW2.Input;
using GNW2.Projectile;
using GNW2.UI;
using Unity.VisualScripting;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace GNW2.Player
{
    public class Player : NetworkBehaviour, ICombat
    {
        [SerializeField] private float speed = 5f;
        [SerializeField] private BulletProjectile bulletPrefab;
        [SerializeField] private float fireRate = 0.1f;
        [Networked] private TickTimer fireDelayTimer { get; set; }
        [SerializeField] private Animator _playerAnimator;
        private Vector3 _bulletSpawnLocation = Vector3.forward * 2;
        private NetworkCharacterController _cc;
        private ChatUI _chatUI;
        private static readonly int IsMoving = Animator.StringToHash("IsMoving");
        private static readonly int Jump = Animator.StringToHash("Jump");

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

            _playerAnimator = GetComponentInChildren<Animator>();
        }

        private void Update()
        {
            
        }

        public override void FixedUpdateNetwork()
        {
            if (!GetInput(out NetworkInputData data)) return;

            if (!HasStateAuthority) return;
            
            MovePlayer(data);
            FireProjectile(data);
            JumpAction(data);
        }
#region Player Input
        private void JumpAction(NetworkInputData data)
        {
            if (data.buttons.IsSet(NetworkInputData.ISJUMP) && _cc.Grounded)
            {
                _cc.Jump();
                
                if (_playerAnimator)
                {
                    _playerAnimator.SetTrigger(Jump);
                }
            }
        }

        private void MovePlayer(NetworkInputData data)
        {
            data.Direction.Normalize();
            _cc.Move(speed *data.Direction * Runner.DeltaTime);
            
            if (_playerAnimator)
            {
                _playerAnimator.SetBool(IsMoving, data.Direction != Vector3.zero);
            }
        }

        private void FireProjectile(NetworkInputData data)
        {
            if (fireDelayTimer.ExpiredOrNotRunning(Runner))
            {

                if (data.Direction.sqrMagnitude > 0)
                {
                    _bulletSpawnLocation = data.Direction * 2f;
                }



                if (data.buttons.IsSet(NetworkInputData.MOUSEBUTTON0))
                {

                    fireDelayTimer = TickTimer.CreateFromSeconds(Runner, fireRate);
                    Runner.Spawn(bulletPrefab, transform.position + _bulletSpawnLocation,
                        Quaternion.LookRotation(_bulletSpawnLocation), Object.InputAuthority,
                        OnBulletSpawned);
                }
            }
        }
        
        private void OnBulletSpawned(NetworkRunner runner, NetworkObject bullet)
        {
            bullet.GetComponent<BulletProjectile>()?.Init();
        }
#endregion

        


        public void TakeDamage(int Damage)
        {
            OnTakeDamage?.Invoke(Damage);
        }
        
        [Rpc(RpcSources.All, RpcTargets.All, InvokeLocal = false)]
        private void RPC_SendMessage(string message)
        {
            Debug.Log(message);
        }
    }
}
