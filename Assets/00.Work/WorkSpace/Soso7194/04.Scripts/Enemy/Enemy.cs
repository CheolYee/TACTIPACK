using _00.Work.WorkSpace.Soso7194._04.Scripts.SO;
using UnityEditor;
using UnityEngine;

namespace _00.Work.WorkSpace.Soso7194._04.Scripts.Enemy
{
    public class Enemy : MonoBehaviour
    {
        private int _hp;
        private int _attack;
        private EnemiesSpawn _spawner;

        public void Setup(EnemySO data, EnemiesSpawn spawner)
        {
            _hp = data.maxHP;
            _attack = data.attack;
            _spawner = spawner;
            gameObject.name = data.name;
        }

        public void TakeDamage(int damage)
        {
            _hp -= damage;
            if (_hp <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            _spawner.RemoveEnemy(gameObject);
            Destroy(gameObject);
        }
    }
}
