using System;
using System.Collections;
using System.Collections.Generic;
using _00.Work.WorkSpace.Soso7194._04.Scripts.Enemy;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour
{
    [Header("Drop Slot")]
    public DropAbleUI[] dropSlots;

    [Header("Manager")]

    public RectTransform[] dragAbleUI; 

    private CharacterSelectManager _CharacterSelectManager;
    
    [Header("적 타겟팅 시스템")]
    [SerializeField] private RandomTargeting enemyTargeting;
    
    public event Action OnPlayerTurnEnd;
    
    private bool _isPlayerTurn = true;

    private void Awake()
    {
        _CharacterSelectManager = GetComponent<CharacterSelectManager>();
    }

    private void OnEnable()
    {
        enemyTargeting.OnEnemyTurnEnd += OnEnemyTurnFinished;
    }

    private void OnDisable()
    {
        enemyTargeting.OnEnemyTurnEnd -= OnEnemyTurnFinished;
    }
    public void OnTurnStart()//시작 버튼
    {
        if (!_isPlayerTurn) return;
        if (_CharacterSelectManager == null) return;

        StartCoroutine(PlayerAttackRoutine());
    }
    private IEnumerator PlayerAttackRoutine()
    {
        Debug.Log("=== 플레이어 턴 시작 ===");
        _isPlayerTurn = false;//플레이어 턴 시작 됐으니 false
        for (int i = 0; i < dropSlots.Length; i++)//할당한 dropSlots 만큼 순회
        {
            if (dropSlots[i].transform.childCount > 0) //자식이 있는지 없는지 체크
            {
                //있다면?
                GameObject icon = dropSlots[i].transform.GetChild(0).gameObject;// 아이콘에 dropSlots의 첫번째 자식 게임 오브젝트의 정보를 담음
                string characterName = icon.name; //스트링 지역 변수 하나 선언하고 icon 오브젝트의 이름으로 설정
                Debug.Log($"{characterName} : 공격");// 누가 공격 했는지 출력
            }
            else
                Debug.Log($"슬롯 {i + 1} : 비어 있음");
              yield return new WaitForSeconds(0.5f); while (transform.childCount > 1)
            {
                Destroy(transform.GetChild(0).gameObject);
            }
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
        for (int i = 0; i < dropSlots.Length; i ++)
        {
            if (dropSlots[i].transform.childCount > 0)
                Destroy(dropSlots[i].transform.GetChild(0).gameObject);
        }
    }
    public void SelectComplected()
    {
        Debug.Log("버튼 실행 됨");

        for (int i = 0; i < _CharacterSelectManager.choiceCharacter.Count; i++)
        {
            var count = _CharacterSelectManager.choiceCharacter[i];//복제 할 거
            var parent = dragAbleUI[i].transform;//위치 

            var clone = Instantiate(count, parent, false);//선택한 캐릭터를 드래그 가능한 UI로 복제

            var rect = clone.GetComponent<RectTransform>();// 복제한 녀석에게서 컴포넌트를 가져오고
            
            if (rect != null)
            {
                rect.position = parent.transform.position;
                rect.localRotation = Quaternion.identity;
                rect.localScale = parent.localScale * 1.5f;
                clone.AddComponent<DragAbleUI>();
                Destroy(clone.GetComponent<CharacterSelect>());
                clone.GetComponent<Image>().color = Color.white;
            }
            Debug.Log($"{clone.name} 소환 완료 (부모: {parent.name})");
        }
    }
}
