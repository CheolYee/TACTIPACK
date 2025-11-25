using System;
using System.Collections.Generic;
using _00.Work.Resource.Scripts.Managers;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Creatures.Players;
using _00.Work.WorkSpace.CheolYee._04.Scripts.UI;
using UnityEngine;

namespace _00.Work.WorkSpace.Hyeonjin0209._01.Scripts.System.Character
{
    public class CharacterSelectManager : MonoBehaviour
    {
        public static CharacterSelectManager Instance;

        [Header("Selection Limit")]
        [SerializeField] private int maxPartySize = 3;

        [Header("Preview Manager")]
        [SerializeField] private PartyPreviewUi previewManager;

        // 선택된 캐릭터 목록 (논리적인 순서용, 필요하면 유지)
        public List<CharacterSelect> choiceCharacter = new();

        // 어떤 캐릭터가 어느 슬롯에 들어갔는지
        private Dictionary<CharacterSelect, int> _slotByCharacter = new();
        private bool[] _slotUsed;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                _slotUsed = new bool[maxPartySize];
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            SoundManager.Instance.PlayBgm(BgmId.Select);
        }

        /// <summary>
        /// 캐릭터 선택/해제 토글
        /// 반환값: true = 선택된 상태, false = 해제된 상태
        /// </summary>
        public bool SelectCharacter(CharacterSelect select)
        {
            if (select == null) return false;

            // 이미 선택되어 있다면 → 해제
            if (_slotByCharacter.TryGetValue(select, out int slotIndex))
            {
                DeselectInternal(select, slotIndex);
                return false;
            }

            // 새로 선택
            if (choiceCharacter.Count >= maxPartySize)
            {
                // 정원 초과
                return false;
            }

            int freeSlot = FindFirstFreeSlot();
            if (freeSlot < 0)
            {
                // 이론상 올 일은 없음 (maxPartySize랑 동기화)
                return false;
            }

            choiceCharacter.Add(select);
            _slotByCharacter[select] = freeSlot;
            _slotUsed[freeSlot] = true;

            // 프리뷰 슬롯에 해당 캐릭터의 애니메이터 세팅
            if (previewManager != null && select.CharacterData != null)
            {
                previewManager.SetSlot(freeSlot, select.CharacterData);
            }

            return true;
        }

        private void DeselectInternal(CharacterSelect select, int slotIndex)
        {
            // 리스트에서도 제거
            choiceCharacter.Remove(select);

            // 슬롯 정보 초기화
            _slotByCharacter.Remove(select);
            if (slotIndex >= 0 && slotIndex < _slotUsed.Length)
            {
                _slotUsed[slotIndex] = false;
            }

            // 프리뷰 슬롯 숨기기
            if (previewManager != null)
            {
                previewManager.SetSlot(slotIndex, null);
            }
        }

        private int FindFirstFreeSlot()
        {
            for (int i = 0; i < _slotUsed.Length; i++)
            {
                if (!_slotUsed[i])
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// 현재 선택된 캐릭터의 PlayerDefaultData 리스트 (순서는 choiceCharacter 기준)
        /// </summary>
        public List<PlayerDefaultData> GetSelectedPartyData()
        {
            var list = new List<PlayerDefaultData>();

            foreach (var cs in choiceCharacter)
            {
                if (cs != null && cs.CharacterData != null)
                    list.Add(cs.CharacterData);
            }

            return list;
        }
    }
}
