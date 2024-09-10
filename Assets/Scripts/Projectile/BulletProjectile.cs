using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace GNW2.Projectile
{
    public class BulletProjectile : NetworkBehaviour
    {
        [SerializeField] private float bulletSpeed = 10f;
        [SerializeField] private float lifeTime = 5f;
        [Networked]private TickTimer life { get; set; }

        public void Init()
        {
            life = TickTimer.CreateFromSeconds(Runner, lifeTime);
        }

        public override void FixedUpdateNetwork()
        {
            if (life.Expired(Runner))
            {
                Runner.Despawn(Object);
            }
            else
            {
                transform.position += bulletSpeed * transform.forward * Runner.DeltaTime;
            }
        }
    }
}
