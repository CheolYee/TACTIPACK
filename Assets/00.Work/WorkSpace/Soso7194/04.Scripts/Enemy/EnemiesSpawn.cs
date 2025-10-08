using System.Collections.Generic;
using _00.Work.WorkSpace.Soso7194._04.Scripts.SO;
using UnityEngine;

namespace _00.Work.WorkSpace.Soso7194._04.Scripts.Enemy
{
    public class EnemiesSpawn : MonoBehaviour
    {
        public List<GameObject> SpawnedEnemies { get; private set; }
        
        [Header("적 SO")]
        [SerializeField] private List<EnemySO> enemyDatas;
        
        [Header("적 스폰 포인트")]
        [SerializeField] private List<Transform> enemiesSpawnPos;

        private void Start()
        {
            SpawnedEnemies = new List<GameObject>();

            int spawnCount = Random.Range(1, 4);

            List<Transform> availablePositions = new List<Transform>(enemiesSpawnPos);

            for (int i = 0; i < spawnCount; i++)
            {
                EnemySO data = enemyDatas[Random.Range(0, enemyDatas.Count)];

                int index = Random.Range(0, availablePositions.Count);
                Transform spawnPos = availablePositions[index];
                availablePositions.RemoveAt(index);

                GameObject enemy = Instantiate(data.prefab, spawnPos.position, spawnPos.rotation);

                Enemy enemyCompo = enemy.GetComponent<Enemy>();
                if (enemyCompo != null)
                {
                    enemyCompo.Setup(data, this);
                }

                SpawnedEnemies.Add(enemy);
            }
            
            Debug.Log($"적 {SpawnedEnemies.Count}명 스폰 완료");
        }

        public void RemoveEnemy(GameObject enemy)
        {
            if (SpawnedEnemies.Contains(enemy))
            {
                SpawnedEnemies.Remove(enemy);
            }
        }
    }
}