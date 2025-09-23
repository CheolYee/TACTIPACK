using System;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Projectiles.Laser
{
    public class LaserAnimEvents : MonoBehaviour
    {
        private Laser _laser;

        private void Awake()
        {
            _laser = GetComponentInParent<Laser>();
        }

        public void LaserDamage()
        {
            _laser.LaserDamageCaster();
        }
    }
}