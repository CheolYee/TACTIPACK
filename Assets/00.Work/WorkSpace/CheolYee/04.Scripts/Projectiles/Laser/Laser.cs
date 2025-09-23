using System;
using System.Collections;
using _00.Work.Scripts.Managers;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Creatures.Attack;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Projectiles.Laser
{
    public class Laser : Projectile
    {
        private static readonly int Attack = Animator.StringToHash("ATTACK");

        [Header("Laser Settings")]
        [SerializeField] private float lifeTime = 2f;
        [SerializeField] private DamageCaster damageCaster;
        
        private float _damage; //데미지
        private float _knockBackPower; //넉백파워
        
        private Animator _animator;

        public override void Initialize(Transform firepos, Vector2 direction, float damage, float knockBackPower, float speed)
        {
            _damage = damage;
            _knockBackPower = knockBackPower;
            
            transform.position = firepos.position;
            StartCoroutine(LaserStart());
        }

        protected override void Awake()
        {
            base.Awake();
            _animator = GetComponentInChildren<Animator>();
        }

        private void OnValidate()
        {
            itemName = gameObject.name;
        }

        private IEnumerator LaserStart()
        {
            _animator.SetBool(Attack, true);
            yield return new WaitForSeconds(lifeTime);
            _animator.SetBool(Attack, false);
            PoolManager.Instance.Push(this);
        }

        public void LaserDamageCaster()
        {
            damageCaster.CastDamage(_damage, _knockBackPower);
        }
    }
}
