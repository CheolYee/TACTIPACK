using System.Collections.Generic;
using _00.Work.Scripts.Managers;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Creatures.Enemies;
using _00.Work.WorkSpace.JaeHun._01._Scrpits;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Stages
{
    public class EnemySpawnManager : MonoSingleton<EnemySpawnManager>
    {
        [Header("Enemies")]
        [SerializeField] private Enemy enemyPrefab;
        [SerializeField] private List<Transform> spawnPoints;
        
        [Header("RunTimeData")]
        [SerializeField] private List<EnemyDefaultData> selectedEnemyData = new();
        [SerializeField] private List<Enemy> enemies = new();
        

        public void SetSpawnEnemy(MapSo map)
        {
            SpawnEnemyFromData(map);
        }

        private void SpawnEnemyFromData(MapSo map)
        {
            if (enemyPrefab == null || spawnPoints.Count == 0)
            {
                Debug.LogError("[EnemySpawnManager] 스폰 설정이 잘못되었습니다.");
            }

            ClearEnemy();
            
            List<EnemyDefaultData> enemyData = new();
            map.stageData.enemies
                .ForEach(e => enemyData.Add(e.enemyData));
            
            int count = Mathf.Min(enemyData.Count, spawnPoints.Count);
            for (int i = 0; i < count; i++)
            {
                var data = enemyData[i];
                var spawnPoint = spawnPoints
                    [map.stageData.enemies[i].spawnIndex];
                
                if (data == null || spawnPoint == null) continue;
                
                Enemy instance = Instantiate(enemyPrefab, spawnPoint.position,spawnPoint.rotation);
                instance.name = $"[적군 {i} {data.EnemyName}]";
                
                instance.SetupEnemy(data);
                enemies.Add(instance);
            }
        }

        private void ClearEnemy()
        {
            foreach (Enemy enemy in enemies)
            {
                if (enemy != null)
                    Destroy(enemy.gameObject);
            }
            selectedEnemyData.Clear();
            enemies.Clear();
        }
    }
}