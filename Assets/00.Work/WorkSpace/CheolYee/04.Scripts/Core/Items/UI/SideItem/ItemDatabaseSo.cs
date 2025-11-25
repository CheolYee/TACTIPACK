using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.UI.SideItem
{
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "SO/Item/Item Database", order = 0)]
    public class ItemDatabaseSo : ScriptableObject
    {
        [Header("Side Item Database")]
        [field: SerializeField] public List<ItemDataSo> ItemDatabase { get; private set; }
        
        public ItemDataSo GetItemById(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            return ItemDatabase.FirstOrDefault(i => i != null && i.itemId == id);
        }
    }
}