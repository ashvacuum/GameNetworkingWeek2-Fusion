using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using GNW2.Input;
using GNW2.Projectile;
using Unity.VisualScripting;
using UnityEngine;

namespace GNW2.Player
{
    public class Player : NetworkBehaviour
    {
        [SerializeField] private float speed = 5f;
        [SerializeField] private BulletProjectile bulletPrefab;
        [SerializeField] private float fireRate = 0.1f;
        [Networked] private TickTimer fireDelayTimer { get; set; }
        private Vector3 _bulletSpawnLocation = Vector3.forward * 2;
        private NetworkCharacterController _cc;
        private float? speeeeed = null;
        private event Action OnButtonPressed;
        private void Awake()
        {
            OnButtonPressed?.Invoke();
            speeeeed ??= 2f;
            if (!speeeeed.HasValue)
            {
                speeeeed = 2f;
            }
            _cc = GetComponent<NetworkCharacterController>();
        }

        public override void FixedUpdateNetwork()
        {
            if (!GetInput(out NetworkInputData data)) return;
            
            data.Direction.Normalize();
            _cc.Move(speed *data.Direction * Runner.DeltaTime);

            if (!HasStateAuthority || !fireDelayTimer.ExpiredOrNotRunning(Runner)) return;
            
            if (data.Direction.sqrMagnitude > 0)
            {
                _bulletSpawnLocation = data.Direction * 2f;
            }

            if (!data.buttons.IsSet(NetworkInputData.MOUSEBUTTON0)) return;
                
            fireDelayTimer = TickTimer.CreateFromSeconds(Runner, fireRate);
            Runner.Spawn(bulletPrefab, transform.position + _bulletSpawnLocation,
                Quaternion.LookRotation(_bulletSpawnLocation), Object.InputAuthority,
                (runner, bullet) =>
                {
                    bullet.GetComponent<BulletProjectile>()?.Init();
                });
        }

        private void OnBulletSpawned(NetworkRunner runner, NetworkObject bullet)
        {
            bullet.GetComponent<BulletProjectile>()?.Init();
        }
        
        
    }
}
