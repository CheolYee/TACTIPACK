using System.Collections.Generic;
using _00.Work.WorkSpace.Soso7194._04.Scripts.SO;
using _00.Work.WorkSpace.Soso7194._04.Scripts.Json;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _00.Work.WorkSpace.Soso7194._04.Scripts.Enemy
{
    public class EnemiesSpawn : MonoBehaviour
    {
        public List<GameObject> SpawnedEnemies { get; private set; } // 에너미 저장

        [Header("Enemy Spawns")]
        [Range(1, 3)]
        [SerializeField] private int enemySpawns = 1;
        
        [Header("Enemy SO")]
        [SerializeField] private List<EnemySO> enemyData; // 에너미 SO
        
        [Header("Enemies Spawn Pos")]
        [SerializeField] private List<Transform> enemiesSpawnPos; // 에너미 스폰포인트
        
        [Header("적 HP바")]
        [SerializeField] private GameObject prefabHpBar; // HP바 프리팹
        [SerializeField] private GameObject hpBarParent; // 캔버스 선택
        
        private RandomTargeting _targeting;
        private EnemySave _enemySave;
        
        private void Awake()
        {
            // 같은 오브젝트 안의 RandomTargeting 가져오기
            _targeting = GetComponent<RandomTargeting>();
            if (_targeting == null)
                Debug.LogWarning("[EnemiesSpawn] 같은 오브젝트에 RandomTargeting 컴포넌트가 없습니다.");

            // EnemySave 컴포넌트 가져오기 또는 추가
            _enemySave = GetComponent<EnemySave>();
            if (_enemySave == null)
                _enemySave = gameObject.AddComponent<EnemySave>();
        }
        
        private void Start()
        {
            // 저장 초기화
            SpawnedEnemies = new List<GameObject>();

            // 저장된 데이터가 있으면 로드하고, 없으면 새로 스폰한다
            if (_enemySave.HasSaveData())
            {
                LoadSavedEnemies();
            }
            else
            {
                SpawnNewEnemies();
            }
            
            Debug.Log($"적 {SpawnedEnemies.Count}명 준비 완료");
            if (_targeting != null)
            {
                _targeting.SetEnemies(SpawnedEnemies);
                Debug.Log("[EnemiesSpawn] RandomTargeting에 적 리스트 전달 완료");
            }
        }

        private void LoadSavedEnemies()
        {
            EnemySave.SaveData saveData = _enemySave.LoadEnemies(enemyData);

            // 스폰 포인트 인덱스 추적
            int spawnIndex = 0;

            foreach (var savedEnemy in saveData.enemies)
            {
                if (savedEnemy.enemySO == null)
                {
                    Debug.LogWarning($"저장된 적 SO '{savedEnemy.enemySOName}'를 찾을 수 없습니다.");
                    continue;
                }

                // 스폰 포인트 선택 (순환)
                Transform spawnPos = enemiesSpawnPos[spawnIndex % enemiesSpawnPos.Count];
                spawnIndex++;

                // SO에서 프리팹 가져와서 생성 (스폰 포인트의 자식으로)
                GameObject enemy = Instantiate(savedEnemy.enemySO.prefab, savedEnemy.position, Quaternion.identity, spawnPos);

                // 에너미 정보를 에너미 코드에 저장 및 HP바 생성
                Enemy enemyCompo = enemy.GetComponent<Enemy>();
                if (enemyCompo != null)
                {
                    // SO 데이터로 Setup
                    enemyCompo.Setup(savedEnemy.enemySO, this);
                    
                    // 저장된 HP 값으로 덮어쓰기
                    Debug.Log($"적 로드: {savedEnemy.enemySOName}, 현재HP: {savedEnemy.currentHP}, 최대HP: {savedEnemy.maxHp}");
                    enemyCompo.maxHP = savedEnemy.maxHp;
                    enemyCompo.CurrentHP = savedEnemy.currentHP;
                    
                    // HP바 생성 및 설정
                    GameObject hpBarObj = Instantiate(prefabHpBar, hpBarParent.transform);
                    RectTransform hpBarRect = hpBarObj.GetComponent<RectTransform>();
                    
                    // 에너미 HP바 설정
                    enemyCompo.SetHpBar(hpBarRect);
                    
                    // HP바 업데이트 (현재 HP에 맞춰)
                    //enemyCompo.TakeDamage(0); // HP바를 현재 HP에 맞춰 갱신
                }
                else
                {
                    Debug.LogError("에너미 스크립트 부재");
                }

                // 스폰한 에너미 리스트에 저장
                SpawnedEnemies.Add(enemy);
            }

            Debug.Log($"저장된 적 {SpawnedEnemies.Count}명 로드 완료");
        }

        private void SpawnNewEnemies()
        {
            // 스폰 포인트를 리스트로 저장
            List<Transform> availablePositions = new List<Transform>(enemiesSpawnPos);

            for (int i = 0; i < enemySpawns; i++)
            {
                // SO에서 에너미 데이터 랜덤으로 1개 가저옴
                EnemySO data = enemyData[Random.Range(0, enemyData.Count)];

                // 스폰할 위치를 지정된 스폰포인트중 랜덤으로 지정
                int index = Random.Range(0, availablePositions.Count);
                // 에너미가 스폰할 트랜스폼을 저장
                Transform spawnPos = availablePositions[index];
                // 리스트에서 지정된 스폰포인트 삭제
                availablePositions.RemoveAt(index);

                // 에너미 생성 (스폰 포인트의 자식으로)
                GameObject enemy = Instantiate(data.prefab, spawnPos.position, Quaternion.identity, spawnPos);

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
            
            Debug.Log($"새로운 적 {SpawnedEnemies.Count}명 스폰 완료");

            // 스폰된 적들을 저장 (enemyData 리스트 전달)
            _enemySave.SaveEnemies(SpawnedEnemies, enemyData);
        }
        
        public void RemoveEnemy(GameObject enemy)
        {
            // 리스트에 적이 있으면
            if (SpawnedEnemies.Contains(enemy))
            {
                // 받아온 적을 리스트에서 지운다.
                SpawnedEnemies.Remove(enemy);
                // 만약 적이 다 지워젔다면
                if (SpawnedEnemies.Count == 0)
                {
                    // JSON 파일을 삭제한다.
                    _enemySave.DeleteSaveFile();
                }
                else
                {
                    // 아니면 이 정보를 저장한다.
                    _enemySave.SaveEnemies(SpawnedEnemies, enemyData);
                }
            }
        }
    }
}