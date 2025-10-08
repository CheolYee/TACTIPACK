using UnityEngine;

namespace _00.Work.WorkSpace.Soso7194._04.Scripts.Enemy
{
    public class EnemyManager : MonoBehaviour
    {
        [SerializeField] private EnemiesSpawn spawner;
        [SerializeField] private RandomTargeting targeting;

        private void Start()
        {
            targeting.SetEnemies(spawner.SpawnedEnemies);
        }
    }
}
