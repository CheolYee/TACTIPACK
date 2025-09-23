using _00.Work.Scripts.SO;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Projectiles
{
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class Projectile : MonoBehaviour, IPoolable
    {
        [SerializeField] protected string itemName;
        public string ItemName => itemName;
        public GameObject GameObject => gameObject;
        
        protected bool IsDead;
        protected float Timer;
        
        protected Rigidbody2D RbCompo;
        
        protected virtual void Awake()
        {
            RbCompo = GetComponent<Rigidbody2D>();
        }
        
        public abstract void Initialize(Transform firepos, Vector2 direction, float damage, float knockBackPower, float speed);
        public void ResetItem()
        {
            IsDead = false;
            Timer = 0;
        }
    }
}