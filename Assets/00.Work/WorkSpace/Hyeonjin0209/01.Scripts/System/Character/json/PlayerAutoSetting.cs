using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct CharacterPrefabEntry
{
    public CharacterType type;
    public GameObject prefab;
}

public class PlayerAutoSetting : MonoBehaviour
{
    [SerializeField] private RectTransform[] SettingIconUI;
    [SerializeField] private Transform[] SpawnPoints;
    [SerializeField] private List<CharacterPrefabEntry> prefabEntries;

    private Dictionary<CharacterType, GameObject> prefabMap;
    private SaveData partyData;

    private void Awake()
    {
        prefabMap = new Dictionary<CharacterType, GameObject>();
        foreach (var entry in prefabEntries)
            prefabMap[entry.type] = entry.prefab;

        // 저장된 데이터 불러오기
        partyData = PartyInfo.Load("party_002");
    }

    private void Start()
    {
        var c = partyData.party; // List<CharacterType>

        for (int i = 0; i < c.Count; i++)
        {
            var type = c[i];                        // 복제 할 녀석(이미 party에 담겨져 있음)
            var parent = SettingIconUI[i].transform;   // 슬롯 위치
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
    }
}
