using System;
using System.Collections.Generic;
using System.IO;
using _00.Work.Scripts.Managers;
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
            public Vector3 position; // 위치
            public int currentHP; // 현재 HP
            public int maxHp; // 최대 HP
            
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
                        EnemySO matchedSO = allEnemySOs.Find(so => so.prefab.name == enemyName || so.name == enemyName);
                        
                        if (matchedSO != null)
                        {
                            EnemyData enemyData = new EnemyData
                            {
                                enemySOName = matchedSO.name, // SO의 이름 저장
                                position = enemyObj.transform.position, // 위치 저장
                                currentHP = enemyComponent.CurrentHP, // 현재 체력 저장
                                maxHp = enemyComponent.maxHP // 최대 체력 저장
                            };

                            Debug.Log($"적 저장: {matchedSO.name}, 현재HP: {enemyData.currentHP}, 최대HP: {enemyData.maxHp}");
                            data.enemies.Add(enemyData); // 데이터 리스트에 추가
                        }
                        else
                        {
                            Debug.LogWarning($"적 '{enemyName}'에 해당하는 SO를 찾을 수 없습니다.");
                        }
                    }
                }
            }

            string json = JsonUtility.ToJson(data); // 객체를 JSON 문자열로 변환
            File.WriteAllText(EnemySavePath, json); // 변환한걸 미리 정한 파일 위치에 저장
            Debug.Log($"적 {data.enemies.Count}명 저장 완료: {EnemySavePath}");
        }

        public SaveData LoadEnemies(List<EnemySO> allEnemySOs)
        {
            // 파일이 있으면
            if (File.Exists(EnemySavePath))
            {
                string json = File.ReadAllText(EnemySavePath); // 읽어오기
                SaveData data = JsonUtility.FromJson<SaveData>(json); // 읽어온 정보를 리스트에 저장
                
                // 저장된 이름으로 실제 SO 참조 찾아서 연결
                foreach (var enemyData in data.enemies)
                {
                    EnemySO matchedSO = allEnemySOs.Find(so => so.name == enemyData.enemySOName); // SO 이름 찾기
                    if (matchedSO != null)
                    {
                        enemyData.enemySO = matchedSO; // SO 불러오기
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
                // 저장한 JSON 삭제
                File.Delete(EnemySavePath);
                Debug.Log("적 저장 파일 삭제 완료");
            }
        }

        // JSON 파일이 있는지 확인
        public bool HasSaveData()
        {
            return File.Exists(EnemySavePath);
        }
    }
}