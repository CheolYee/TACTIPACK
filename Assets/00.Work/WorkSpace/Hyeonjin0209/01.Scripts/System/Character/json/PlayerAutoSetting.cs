using System;
using System.Collections.Generic;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Creatures.Players;
using _00.Work.WorkSpace.Hyeonjin0209._01.Scripts.System.Character;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct CharacterPrefabEntry
{
    public CharacterClass type;
    public GameObject prefab;
    public Sprite icon;
}

public class PlayerAutoSetting : MonoBehaviour
{
    [SerializeField] private RectTransform[] settingIconUI;//드래그 앤 드랍 용 캐릭터 UI 스폰 위치
    [SerializeField] private Transform[] SpawnPoints;//캐릭터 스폰 위치
    [SerializeField] private List<CharacterPrefabEntry> prefabEntries; // 딕셔너리

    private Dictionary<CharacterClass, GameObject> prefab;//프리팹 사전
    private Dictionary<CharacterClass, Sprite> sprite;//스프라이트  사전
    private SaveData partyData; // 파티 데이터

    private void Start()
    {
        prefab = new Dictionary<CharacterClass, GameObject>(); // 초기화
        sprite = new Dictionary<CharacterClass, Sprite>();// 초기화
        foreach (var e in prefabEntries)
            prefab[e.type] = e.prefab;//타입 담기(선택한 캐릭터에 따른 픠팹을 생성하기 위해서
        foreach (var o in prefabEntries)
            sprite[o.type] = o.icon;//타입 담기(선택한 캐릭터에 따른 스프라이트를 집어넣기 위해서

        // 저장된 데이터 불러오기
        partyData = PartyInfo.Load<SaveData>("party_002");
        //==========================================================================================//
        var c = partyData.party; //c에다 파티 데이터 담기

        for (int i = 0; i < c.Count; i++)
        {
            var type = c[i];                        // 복제 할 녀석(이미 party에 담겨져 있음)
            var parent = settingIconUI[i].transform;   // 슬롯 위치
            var prefab = this.prefab[type];           // 해당 타입의 프리팹 가져오기

            var clone = Instantiate(prefab, parent, false);


            var rect = clone.GetComponent<RectTransform>();

            rect.position = parent.transform.position;
            rect.localRotation = Quaternion.identity;
            rect.localScale = parent.localScale * 1.5f;
            clone.AddComponent<DragAbleUI>();
            Destroy(clone.GetComponent<CharacterSelect>());
            clone.GetComponent<Image>().color = Color.white;
        }
//=============================================================================================//
        for (int i = 0; i < c.Count; i++)
        {
            var type = c[i];                        // 복제 할 녀석(이미 party에 담겨져 있음)
            var parent = SpawnPoints[i].transform;   // 슬롯 위치
            var prefab = this.prefab[type];           // 해당 타입의 프리팹 가져오기
            var sprite = this.sprite[type];
            var character = Instantiate(prefab, parent, false);

            transform.position = parent.transform.position;
            transform.localRotation = Quaternion.identity;
            transform.localScale = parent.localScale * 1.5f;
            Destroy(character.GetComponent<CharacterSelect>());
            Destroy(character.GetComponent<Image>());
            Destroy(character.GetComponent<CanvasGroup>());
            Destroy(character.GetComponent<CanvasRenderer>());
            var sr = character.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            if (sr == null) Debug.Log("1");
            if (sr.sprite == null) Debug.Log("2");
            if (sprite == null) Debug.Log("3");
        }
    }
}
