using System;
using System.Collections.Generic;
using System.IO;
using _00.Work.WorkSpace.Soso7194._04.Scripts.SO;
using UnityEngine;

namespace _00.Work.WorkSpace.Soso7194._04.Scripts.Json
{
    public class EnemySave : MonoBehaviour
    {
        [Serializable]
        public class SaveData
        {
            public List<EnemyData> enemies = new List<EnemyData>();
        }

        [Serializable]
        public class EnemyData
        {
            public string enemySOName; // SO 이름
            public Vector3 position;
            public Quaternion rotation;
            public int currentHp;
            public int maxHp;
            
            [NonSerialized]
            public EnemySO enemySO; // 로드 시 여기에 SO 참조 저장
        }

        private static string EnemySavePath => Application.persistentDataPath + "/enemyData.json";

        public void SaveEnemies(List<GameObject> spawnedEnemies, List<EnemySO> allEnemySOs)
        {
            SaveData data = new SaveData();

            foreach (GameObject enemyObj in spawnedEnemies)
            {
                if (enemyObj != null)
                {
                    var enemyComponent = enemyObj.GetComponent<Enemy.Enemy>();
                    if (enemyComponent != null)
                    {
                        // 적의 이름으로 원본 SO 찾기
                        string enemyName = enemyObj.name.Replace("(Clone)", "").Trim();
                        EnemySO matchedSO = allEnemySOs.Find(so => so.prefab.name == enemyName);
                        
                        if (matchedSO != null)
                        {
                            EnemyData enemyData = new EnemyData
                            {
                                enemySOName = matchedSO.name, // SO의 이름 저장
                                position = enemyObj.transform.position,
                                rotation = enemyObj.transform.rotation,
                                currentHp = enemyComponent.CurrentHP,
                                maxHp = enemyComponent.maxHP
                            };

                            data.enemies.Add(enemyData);
                        }
                    }
                }
            }

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(EnemySavePath, json);
            Debug.Log($"적 {data.enemies.Count}명 저장 완료: {EnemySavePath}");
            
            Debug.Log(json);
        }

        public SaveData LoadEnemies(List<EnemySO> allEnemySOs)
        {
            if (File.Exists(EnemySavePath))
            {
                string json = File.ReadAllText(EnemySavePath);
                SaveData data = JsonUtility.FromJson<SaveData>(json);
                
                // 저장된 이름으로 실제 SO 참조 찾아서 연결
                foreach (var enemyData in data.enemies)
                {
                    EnemySO matchedSO = allEnemySOs.Find(so => so.name == enemyData.enemySOName);
                    if (matchedSO != null)
                    {
                        enemyData.enemySO = matchedSO;
                    }
                    else
                    {
                        Debug.LogWarning($"SO '{enemyData.enemySOName}'을 찾을 수 없습니다.");
                    }
                }
                
                Debug.Log($"적 {data.enemies.Count}명 로드 완료");
                return data;
            }
            else
            {
                Debug.Log("저장된 적 데이터가 없습니다.");
                return new SaveData();
            }
        }

        public void DeleteSaveFile()
        {
            if (File.Exists(EnemySavePath))
            {
                File.Delete(EnemySavePath);
                Debug.Log("적 저장 파일 삭제 완료");
            }
        }

        public bool HasSaveData()
        {
            return File.Exists(EnemySavePath);
        }
    }
}