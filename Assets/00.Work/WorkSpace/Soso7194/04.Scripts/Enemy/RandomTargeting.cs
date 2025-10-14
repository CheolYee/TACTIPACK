using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
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
                Sequence seq = DOTween.Sequence();
                Vector3 startPos = enemy.transform.position;

                int targetIndex = Random.Range(0, targets.Count);
                seq.Append(enemy.transform.DOMove(targets[targetIndex].transform.position + new Vector3(0.5f, 0, 0), 0.3f));
                Debug.Log($"{enemy.name} 때림");

                yield return new WaitForSeconds(1f);

                seq.Append(enemy.transform.DOMove(startPos, 0.3f));
                yield return new WaitForSeconds(1f);
            }

            _stop = false;
            Debug.Log("=== 적 턴 종료 ===");
            OnEnemyTurnEnd?.Invoke();
        }
    }
}
