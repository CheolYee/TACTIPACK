using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct CharacterPrefabEntry
{
    public CharacterType type;
    public GameObject prefab;
    public Sprite icon;
}

public class PlayerAutoSetting : MonoBehaviour
{
    [SerializeField] private RectTransform[] settingIconUI;
    [SerializeField] private Transform[] SpawnPoints;
    [SerializeField] private List<CharacterPrefabEntry> prefabEntries;

    private Dictionary<CharacterType, GameObject> prefabMap;
    private Dictionary<CharacterType, Sprite> spriteMap;
    private SaveData partyData;

    private void Start()
    {
        prefabMap = new Dictionary<CharacterType, GameObject>();
        spriteMap = new Dictionary<CharacterType, Sprite>();
        foreach (var e in prefabEntries)
            prefabMap[e.type] = e.prefab;
        foreach (var o in prefabEntries)
            spriteMap[o.type] = o.icon;

        // 저장된 데이터 불러오기
        partyData = PartyInfo.Load<SaveData>("party_002");
//==========================================================================================//
        var c = partyData.party; // List<CharacterType>

        for (int i = 0; i < c.Count; i++)
        {
            var type = c[i];                        // 복제 할 녀석(이미 party에 담겨져 있음)
            var parent = settingIconUI[i].transform;   // 슬롯 위치
            var prefab = prefabMap[type];           // 해당 타입의 프리팹 가져오기

            var clone = Instantiate(prefab, parent, false);


            var rect = clone.GetComponent<RectTransform>();

            rect.position = parent.transform.position;
            rect.localRotation = Quaternion.identity;
            rect.localScale = parent.localScale * 1.5f;
            clone.AddComponent<DragAbleUI>();
            Destroy(clone.GetComponent<CharacterSelect>());
            clone.GetComponent<Image>().color = Color.white;
        }

        for (int i = 0; i < c.Count; i++)
        {
            var type = c[i];                        // 복제 할 녀석(이미 party에 담겨져 있음)
            var parent = SpawnPoints[i].transform;   // 슬롯 위치
            var prefab = prefabMap[type];           // 해당 타입의 프리팹 가져오기
            var sprite = spriteMap[type];
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
        }
    }
}
