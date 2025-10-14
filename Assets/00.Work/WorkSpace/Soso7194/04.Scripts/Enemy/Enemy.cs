using _00.Work.WorkSpace.Soso7194._04.Scripts.SO;
using UnityEditor;
using UnityEngine;

namespace _00.Work.WorkSpace.Soso7194._04.Scripts.Enemy
{
    public class Enemy : MonoBehaviour
    {
        public int CurrentHP { get; set; }
        
        private int _maxHp;
        private int _attack;
        private EnemiesSpawn _spawner;

        public void Setup(CharacterDataSO data, EnemiesSpawn spawner)
        {
            _maxHp = data.maxHP;
            _attack = data.attackPower;
            _spawner = spawner;
            gameObject.name = data.name;
        }

        public void TakeDamage(int damage)
        {
            CurrentHP -= damage;
            if (CurrentHP <= 0)
            {
                Die();
            }
        }


        [field:SerializeField] public CharacterDataSO Data { get; set; }
        public void Attack()
        {
            throw new System.NotImplementedException();
        }

        public void EatHealItem()
        {
            throw new System.NotImplementedException();
        }

        public void EnemySkill()
        {
            throw new System.NotImplementedException();
        }
        
        private void Die()
        {
            _spawner.RemoveEnemy(gameObject);
            Destroy(gameObject);
        }
    }
}
