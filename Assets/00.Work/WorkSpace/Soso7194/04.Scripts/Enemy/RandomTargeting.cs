using System;
using System.Collections.Generic;
using _00.Work.WorkSpace.Soso7194._04.Scripts.Json;
using DG.Tweening;
using MEC;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _00.Work.WorkSpace.Soso7194._04.Scripts.Enemy
{
    public class RandomTargeting : MonoBehaviour
    {
        
        [Header("Targeting")]
        [SerializeField] private List<GameObject> targets;

        private List<GameObject> _enemies;
        private bool _stop;

        public event Action OnEnemyTurnEnd;

        public void SetEnemies(List<GameObject> spawnedEnemies)
        {
            _enemies = spawnedEnemies;
            Debug.Log($"[RandomTargeting] 적 {_enemies.Count}명 세팅 완료");
            foreach (var v in _enemies)
            {
                Debug.Log($"- {v.name}");
            }
        }

        public void StartTargeting()
        {
            if (_enemies == null || _enemies.Count == 0)
            {
                Debug.LogWarning("[RandomTargeting] 적 리스트가 비어 있음");
                return;
            }

            if (!_stop)
            {
                _stop = true;
                Timing.RunCoroutine(Targeting());
            }
        }

        private IEnumerator<float> Targeting()
        {
            Debug.Log("=== 적 턴 시작 ===");

            List<GameObject> enemiesCopy = new List<GameObject>(_enemies);
            List<GameObject> targetsCopy = new List<GameObject>(targets);

            foreach (var enemy in enemiesCopy)
            {
                if (enemy == null)
                {
                    Debug.Log("적이 이미 파괴됨, 스킵");
                    continue;
                }

                var enemyScript = enemy.GetComponent<Enemy>();
                Sequence seq = DOTween.Sequence();
                Vector3 startPos = enemy.transform.position;

                int targetIndex;
                targetIndex = Random.Range(0, targetsCopy.Count);
                /*while (true)
                {
                    targetIndex = Random.Range(0, targetsCopy.Count);
                    if (targets[targetIndex].GetComponentInChildren<IPlayer>() != null)
                        break;
                }*/

                seq.Append(enemy.transform.DOMove(targets[targetIndex].transform.position + new Vector3(0.5f, 0, 0), 0.3f));
                enemyScript.TakeDamage(5);
                Debug.Log($"{enemy.name}이(가) {targets[targetIndex].name}을(를) 공격");

                yield return Timing.WaitForSeconds(1f);

                if (enemy != null)
                {
                    seq.Append(enemy.transform.DOMove(startPos, 0.3f));
                    yield return Timing.WaitForSeconds(1f);
                }
                else
                {
                    Debug.Log("적이 사망하여 돌아가지 않음");
                }
            }
            
            _stop = false;
            Debug.Log("=== 적 턴 종료 ===");
            OnEnemyTurnEnd?.Invoke();
        }
    }
}
