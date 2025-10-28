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
        [SerializeField] private List<GameObject> targets; // 플레이어 파티 스폰 위치

        private List<GameObject> _enemies; // 에너미
        private bool _stop; // 중간에 중복으로 실행되는걸 막기

        public event Action OnEnemyTurnEnd; //턴 끝난걸 호출

        // 공격할 에너미들 세팅
        public void SetEnemies(List<GameObject> spawnedEnemies)
        {
            _enemies = spawnedEnemies;
        }

        // 타겟팅 시작하기전 문제 확인
        public void StartTargeting()
        {
            if (_enemies == null || _enemies.Count == 0)
            {
                // 나중에 다음 스테이지로 넘어가는걸 구현해야함
                Debug.LogWarning("적 리스트가 비어 있음");
                return;
            }

            if (!_stop)
            {
                // 문제가 없다면, 중복으로 시작을 막고 타겟팅으로 이동
                _stop = true;
                StartCoroutine(Targeting());
            }
        }

        private IEnumerator Targeting()
        {
            Debug.Log("=== 적 턴 시작 ===");

            // 리스트의 복사본을 만들어서 순회
            List<GameObject> enemiesCopy = new List<GameObject>(_enemies);
            // 플레이어 스폰포인트도 복사본 만들기
            List<GameObject> targetsCopy = new List<GameObject>(targets);

            foreach (var enemy in enemiesCopy)
            {
                // 혹시 모르니 여기 안에도 에너미가 없는 상황 대비
                if (enemy == null)
                {
                    Debug.Log("적이 이미 파괴됨, 스킵");
                    continue;
                }
                
                var enemyScript = enemy.GetComponent<Enemy>(); // 이번 에너미에 에너미 스크립트 가저오기
                Sequence seq = DOTween.Sequence();  // 두트윈 가저오기
                Vector3 startPos = enemy.transform.position; // 처음 위치 저장
                int targetIndex = 0;
                while (true)
                {
                    targetIndex = Random.Range(0, targetsCopy.Count);
                    if (targets[targetIndex].GetComponentInChildren<IPlayer>() != null)
                    {
                        break;
                    }
                }
                seq.Append(enemy.transform.DOMove
                    (targets[targetIndex].transform.position 
                     + new Vector3(0.5f, 0, 0), 0.3f)); // 지정된 플레이어 파티 한명 앞으로 이동
                // 임시로 자기 자신이 데미지 받게 했지만, 나중에 지정된 플레이어한테 데미지 입혀야함
                enemyScript.TakeDamage(5);
                Debug.Log($"{enemy.name} 때림");

                yield return new WaitForSeconds(1f);
                
                if (enemy != null)
                {
                    // 자신 위치로 다시 되돌아가기
                    seq.Append(enemy.transform.DOMove(startPos, 0.3f));
                    yield return new WaitForSeconds(1f);
                }
                else
                {
                    // 지금은 적이 중간에 죽기때문에 예외문 넣음
                    Debug.Log("적이 사망하여 돌아가지 않음");
                }
            }

            _stop = false;
            Debug.Log("=== 적 턴 종료 ===");
            // 에너미턴이 끝난걸 호출
            OnEnemyTurnEnd?.Invoke();
        }

        private void EnemyAttack()
        {
            
        }
    }
}
