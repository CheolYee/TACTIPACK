using _00.Work.WorkSpace.Soso7194._04.Scripts.SO;
using UnityEditor;
using UnityEngine;

namespace _00.Work.WorkSpace.Soso7194._04.Scripts.Enemy
{
    public class Enemy : MonoBehaviour, IEnemy
    {
        [field:SerializeField] public CharacterDataSO Data { get; set; }
        public int CurrentHP { get; set; }
        
        private int _maxHp;
        private int _attack;
        private EnemiesSpawn _spawner;

        public void Setup(EnemySO data, EnemiesSpawn spawner)
        {
            _maxHp = data.maxHP;
            _attack = data.attack;
            _spawner = spawner;
            CurrentHP = _maxHp;
            gameObject.name = data.name;
        }

        public void TakeDamage(int damage)
        {
            CurrentHP -= damage;
            Debug.Log(gameObject.name + " 대미지 입음! " + damage);
            if (CurrentHP <= 0)
            {
                Die();
            }
        }

        public void Attack()
        {
            
        }

        private void Die()
        {
            // 리스트에서 제거는 나중에
            _spawner.RemoveEnemy(gameObject);
            // 즉시 파괴 대신 지연 파괴
            Destroy(gameObject, 0.1f);
        }
    }
}
