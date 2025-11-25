using System;
using System.Collections.Generic;
using _00.Work.Scripts.Managers;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Save;
using UnityEngine;
using Random = System.Random;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.UI.SideItem
{
    [Serializable]
    public class SideInventorySlotData
    {
        public ItemDataSo Item { get; set; }
        public int Count { get; set; }
    }
    
    public class SideInventoryManager : MonoSingleton<SideInventoryManager>
    {
        [Header("SideItem Database")]
        [SerializeField] private ItemDatabaseSo itemDatabase; //모든 아이템을 담고 있는 아이템 데이터베이스

        [Header("Rules")]
        [SerializeField] private int defaultMaxStackItem = 99; //한칸당 최대 중첩 개수

        [Header("Debug")] [SerializeField] private ItemDataSo debugItem;
        
        private readonly Dictionary<string, SideInventorySlotData> _slots = new(); //키는 아이템 아이디, 값은 데이터
        
        public event Action OnInventoryChanged; //인벤이 변경되었을 때 알려주는 이벤트

        #region Save
        public SideInventorySaveData CaptureSaveData()
        {
            var save = new SideInventorySaveData();
        
            foreach (var slot in _slots.Values)
            {
                if (slot == null || slot.Item == null || slot.Count <= 0)
                    continue;

                var entry = new SideInventoryItemSaveEntry
                {
                    dataId = slot.Item.itemId,
                    count  = slot.Count
                };
                save.items.Add(entry);
            }

            return save;
        }

        public ItemDataSo AddRandomItem()
        {
            int range = UnityEngine.Random.Range(0, itemDatabase.ItemDatabase.Count);
            var item = itemDatabase.ItemDatabase[range];
            AddItem(item);
            
            return item;
        }

        //세이브 데이터 로드
        public void ApplySaveData(SideInventorySaveData save)
        {
            // 기존 인벤토리 전부 삭제
            _slots.Clear();
            OnInventoryChanged?.Invoke();

            if (save == null || itemDatabase == null)
                return;

            foreach (var entry in save.items)
            {
                if (string.IsNullOrEmpty(entry.dataId) || entry.count <= 0)
                    continue;

                ItemDataSo item = itemDatabase.GetItemById(entry.dataId);
                if (item == null)
                {
                    Debug.LogWarning($"[SideInventoryManager] 저장된 아이템을 찾을 수 없습니다. dataId={entry.dataId}");
                    continue;
                }

                AddItem(item, entry.count);
            }
        }
        #endregion

        //아이템이 허용되었는가? (사용 가능한가?)
        public bool IsAllowed(ItemDataSo item)
        {
            if (item == null || itemDatabase == null) return false; //아이템 이상하거나 베이스가 없으면 허락 안됨
            return itemDatabase.ItemDatabase.Contains(item); //아이템이 리스트에 들어있다면 true 아니면 false
        }

        //사이드 아이템 인벤토리에 아이템을 개수만큼 추가 후 실제로 추가된 개수를 반환 (기본 1)
        public int AddItem(ItemDataSo item, int count = 1)
        {
            if (item == null || count == 0) return 0; //이상하면 그냥 0리턴
            Debug.Assert(IsAllowed(item), $"아이템이 허용되어있지 않습니다 : {item.itemName}"); //허용된 아이템인지 확인

            if (!_slots.TryGetValue(item.itemId, out SideInventorySlotData slotData)) //딕셔너리에서 가져올 수 없다면
            {
                slotData = new SideInventorySlotData { Item = item , Count = 0 }; //없는거니까 새로 생성
                _slots.Add(item.itemId, slotData);
            }
            
            int space = Math.Max(0, defaultMaxStackItem - slotData.Count); //들어갈 수 있는 공간 크기 구하기
            int toAdd = Math.Min(space, count); //남은 공간만큼 개수 넣도록 계산
            
            if (toAdd <= 0) return 0; //추가할 개수가 0보다 작다면 0개 리턴
            
            slotData.Count += toAdd; //추가될 데이터에 저장
            OnInventoryChanged?.Invoke(); //인벤토리 바뀐거니까 이벤트 실행
            return toAdd; //실 추가 개수 리턴
        }

        //개수만큼 아이템 제거 후 실제 제거된 개수 반환
        public int RemoveItem(ItemDataSo item, int count = 1)
        {
            if (item == null || count == 0) return 0;
            if (!_slots.TryGetValue(item.itemId, out SideInventorySlotData slotData) || slotData.Count <= 0) return 0;
            
            int removeCnt = Mathf.Min(count, slotData.Count); //제거된 개수
            slotData.Count -= removeCnt; //실 저장 데이터에 깎아주기

            if (slotData.Count <= 0) //만약 0개 즉 다 소비했다면?
                _slots.Remove(item.itemId); //딕셔너리에서 제거
            
            if (removeCnt > 0) OnInventoryChanged?.Invoke(); //제거 수가 있다면 이벤이 변경된것이니 실행
            return removeCnt; //리턴
        }

        //모든 아이템 열람 (읽기 위해 + 디버깅용)
        public IEnumerable<SideInventorySlotData> GetAllItems()
        {
            return _slots.Values;
        }

        [ContextMenu("랜덤 아이템 추가 디버그")]
        private void Debug_AddRandomItem()
        {
            var list = itemDatabase.ItemDatabase;
            var pickItem = list[UnityEngine.Random.Range(0, list.Count)];
            int add = AddItem(pickItem);
        }
        
        [ContextMenu("특정 아이템추가 디버그")]
        private void Debug_AddItem()
        {
            AddItem(debugItem);
        }
        
        [ContextMenu("특정 타겟 아이템 제거 디버그")]
        private void Debug_RemoveItemOne()
        {
            RemoveItem(debugItem);
        }
        
        
    }
}