using System.Collections.Generic;
using System.IO;
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
        
        [Header("적 HP바")]
        [SerializeField] private GameObject prefabHpBar;
        [SerializeField] private GameObject hpBarParent;
        
        private static string EnemySavePath => Application.persistentDataPath + "/enemyData.json";
        
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
                    
                    // HP바 생성 및 설정
                    GameObject hpBarObj = Instantiate(prefabHpBar, hpBarParent.transform);
                    RectTransform hpBarRect = hpBarObj.GetComponent<RectTransform>();
                    
                    enemyCompo.SetHpBar(hpBarRect);
                }

                SpawnedEnemies.Add(enemy);
            }
            
            Debug.Log($"적 {SpawnedEnemies.Count}명 스폰 완료");
            //LoadEnemies();
        }
        
        public void RemoveEnemy(GameObject enemy)
        {
            if (SpawnedEnemies.Contains(enemy))
            {
                SpawnedEnemies.Remove(enemy);
                if (SpawnedEnemies.Count == 0)
                {
                    File.Delete(File.Exists(EnemySavePath).ToString());
                }
                else
                {
                    //SaveEnemies();
                }
            }
        }
        
        /*private void LoadEnemies()
        {
            if (File.Exists(EnemySavePath))
            {
                var json = File.ReadAllText(EnemySavePath);
                SpawnedEnemies = JsonUtility.FromJson<List<GameObject>>(json);
            }
            else
            {
                SaveEnemies();
            }
        }

        private void SaveEnemies()
        {
            string json = JsonUtility.ToJson(EnemySavePath);
            File.WriteAllText(EnemySavePath, json);
            Debug.Log(EnemySavePath);
        }*/
    }
}