using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour
{
    [Header("Drop Slot")]
    public DropAbleUI[] dropSlots;
    private IEnumerator PlayerAttackRoutine()
    {
        Debug.Log("=== 플레이어 턴 시작 ===");
        for (int i = 0; i < dropSlots.Length; i++)
        {
            if (dropSlots[i].transform.childCount > 0)
            {
                GameObject icon = dropSlots[i].transform.GetChild(0).gameObject;
                string characterName = icon.name; 
                Debug.Log($"{characterName} : 공격!");
            }
            else
            {
                Debug.Log($"슬롯 {i + 1} : 비어 있음");
            }

              yield return new WaitForSeconds(0.5f); 
        }

        Debug.Log("=== 플레이어 턴 종료 ===");
    }
    public void OnTurnStart()
    {
        if (dropSlots == null) return;

        StartCoroutine(PlayerAttackRoutine());
    }
}
