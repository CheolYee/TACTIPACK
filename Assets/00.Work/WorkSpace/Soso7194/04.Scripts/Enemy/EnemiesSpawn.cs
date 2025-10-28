using System.Collections.Generic;
using System.IO;
using _00.Work.WorkSpace.Soso7194._04.Scripts.SO;
using UnityEngine;
namespace _00.Work.WorkSpace.Soso7194._04.Scripts.Enemy
{
    public class EnemiesSpawn : MonoBehaviour
    {
        public List<GameObject> SpawnedEnemies { get; private set; } // 에너미 저장
        
        [Header("적 SO")]
        [SerializeField] private List<EnemySO> enemyDatas; // 에너미 SO
        
        [Header("적 스폰 포인트")]
        [SerializeField] private List<Transform> enemiesSpawnPos; // 에너미 스폰포인트
        
        [Header("적 HP바")]
        [SerializeField] private GameObject prefabHpBar; // HP바 프리팹
        [SerializeField] private GameObject hpBarParent; // 캔버스 선택
        
        // private static string EnemySavePath => Application.persistentDataPath + "/enemyData.json";
        
        private void Start()
        {
            // 저장 초기화
            SpawnedEnemies = new List<GameObject>();

            // 스폰할 에너미 갯수를 랜덤으로 지정
            int spawnCount = Random.Range(1, 4);

            // 스폰 포인트를 리스트로 저장
            List<Transform> availablePositions = new List<Transform>(enemiesSpawnPos);

            for (int i = 0; i < spawnCount; i++)
            {
                // SO에서 에너미 데이터 랜덤으로 1개 가저옴
                EnemySO data = enemyDatas[Random.Range(0, enemyDatas.Count)];

                // 스폰할 위치를 지정된 스폰포인트중 랜덤으로 지정
                int index = Random.Range(0, availablePositions.Count);
                // 에너미가 스폰할 트랜스폼을 저장
                Transform spawnPos = availablePositions[index];
                // 리스트에서 지정된 스폰포인트 삭제
                availablePositions.RemoveAt(index);

                // 에너미 생성
                GameObject enemy = Instantiate(data.prefab, spawnPos.position, spawnPos.rotation);

                // 에너미 정보를 에너미 코드에 저장 및 HP바 생성
                Enemy enemyCompo = enemy.GetComponent<Enemy>();
                if (enemyCompo != null)
                {
                    // 에너미SO에 있는 정보를 에너미 스크립트에 저장
                    enemyCompo.Setup(data, this);
                    
                    // HP바 생성 및 설정
                    GameObject hpBarObj = Instantiate(prefabHpBar, hpBarParent.transform);
                    RectTransform hpBarRect = hpBarObj.GetComponent<RectTransform>();
                    
                    // 에너미 HP바 설정
                    enemyCompo.SetHpBar(hpBarRect);
                }
                else
                {
                    Debug.LogError("에너미 스크립트 부재");
                }

                // 스폰한 에너미 리스트에 저장
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
                    //File.Delete(File.Exists(EnemySavePath).ToString());
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