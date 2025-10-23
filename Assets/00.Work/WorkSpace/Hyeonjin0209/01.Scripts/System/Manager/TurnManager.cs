using System;
using System.Collections;
using _00.Work.WorkSpace.Soso7194._04.Scripts.Enemy;
using UnityEngine;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour
{
    [Header("Drop Slot")]
    public DropAbleUI[] dropSlots;
    
    [Header("적 타겟팅 시스템")]
    [SerializeField] private RandomTargeting enemyTargeting;
    
    public event Action OnPlayerTurnEnd;
    
    private bool _isPlayerTurn = true;
    
    private void OnEnable()
    {
        enemyTargeting.OnEnemyTurnEnd += OnEnemyTurnFinished;
    }

    private void OnDisable()
    {
        enemyTargeting.OnEnemyTurnEnd -= OnEnemyTurnFinished;
    }
    public void OnTurnStart()
    {
        if (!_isPlayerTurn) return;
        if (dropSlots == null) return;

        StartCoroutine(PlayerAttackRoutine());
    }
    private IEnumerator PlayerAttackRoutine()
    {
        Debug.Log("=== 플레이어 턴 시작 ===");
        _isPlayerTurn = false;
        for (int i = 0; i < dropSlots.Length; i++)
        {
            if (dropSlots[i].transform.childCount > 0)
            {
                GameObject icon = dropSlots[i].transform.GetChild(0).gameObject;
                string characterName = icon.name; 
                Debug.Log($"{characterName} : 공격!");
            }
            else
                Debug.Log($"슬롯 {i + 1} : 비어 있음");
              yield return new WaitForSeconds(0.5f); 
        }
        Debug.Log("=== 플레이어 턴 종료 ===");
        enemyTargeting.StartTargeting();
    }
    private void OnEnemyTurnFinished()
    {
        Debug.Log("적 턴 종료");
        _isPlayerTurn = true;
    }

    public void ResetSlot()
    {
        for (int i = 0; i < dropSlots.Length; i++)
        {
            if (dropSlots[i].transform.childCount > 0)
                Destroy(dropSlots[i].transform.GetChild(0).gameObject);
        }
    }
}
