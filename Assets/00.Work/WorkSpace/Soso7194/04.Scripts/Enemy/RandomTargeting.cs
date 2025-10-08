using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _00.Work.WorkSpace.Soso7194._04.Scripts.Enemy
{
    public class RandomTargeting : MonoBehaviour
    {
        [Header("타겟 위치")]
        [SerializeField] private List<GameObject> targets;

        private List<GameObject> _enemies;
        private bool _stop;

        public event Action OnEnemyTurnEnd;

        public void SetEnemies(List<GameObject> spawnedEnemies)
        {
            _enemies = spawnedEnemies;
        }

        public void StartTargeting()
        {
            if (_enemies == null || _enemies.Count == 0)
            {
                Debug.LogWarning("적 리스트가 비어 있음");
                return;
            }

            if (!_stop)
            {
                _stop = true;
                StartCoroutine(Targeting());
            }
        }

        private IEnumerator Targeting()
        {
            Debug.Log("=== 적 턴 시작 ===");

            foreach (var enemy in _enemies)
            {
                Vector3 startPos = enemy.transform.position;

                int targetIndex = Random.Range(0, targets.Count);
                enemy.transform.position = targets[targetIndex].transform.position + new Vector3(0.5f, 0, 0);
                Debug.Log($"{enemy.name} 때림");

                yield return new WaitForSeconds(0.5f);

                enemy.transform.position = startPos;
                yield return new WaitForSeconds(0.5f);
            }

            _stop = false;
            Debug.Log("=== 적 턴 종료 ===");
            OnEnemyTurnEnd?.Invoke();
        }
    }
}
