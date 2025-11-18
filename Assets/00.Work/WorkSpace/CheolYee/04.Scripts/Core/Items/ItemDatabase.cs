using System.Collections.Generic;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.UI.SideItem;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items
{
    public class ItemDatabase : MonoBehaviour
    {
        public ItemDatabaseSo allItems; //에디터에서 채우기
        private Dictionary<string, ItemDataSo> _itemDict;

        private void Awake()
        {
            _itemDict = new Dictionary<string, ItemDataSo>();
            foreach (ItemDataSo item in allItems.ItemDatabase)
            {
                if (string.IsNullOrEmpty(item.itemId)) Debug.LogWarning($"아이템 데이터 SO {item.name}의 아이디가 없습니다.");
                if (!_itemDict.TryAdd(item.itemId, item)) Debug.LogWarning($"똑같은 아이템이 있습니다. : {item.itemId}");
            }
        }
        
        /// <summary>
        /// 런타임에서 아이템을 초기화 합니다.
        /// </summary>
        public void Initialize()
        {
            _itemDict = new Dictionary<string, ItemDataSo>();
            foreach (ItemDataSo item in allItems.ItemDatabase)
            {
                _itemDict.TryAdd(item.itemId, item);
            }
        }
        

        /// <summary>
        /// 아이디를 바탕으로 아이템을 가져와서 리턴합니다
        /// </summary>
        public ItemDataSo GetItemById(string id)
        {
            if (_itemDict == null || _itemDict.Count == 0)
                Initialize();

            if (_itemDict != null && _itemDict.TryGetValue(id, out ItemDataSo data))
                return data;
            
            Debug.LogWarning($"[itemDataBase] : {id}를 찾을 수 없습니다.");            
            return null;
        }
    }
}