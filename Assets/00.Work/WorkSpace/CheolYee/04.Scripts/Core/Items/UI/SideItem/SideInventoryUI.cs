using System.Collections.Generic;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.UI.SideItem
{
    public class SideInventoryUI : MonoBehaviour
    {
        [Header("Reference")]
        [SerializeField] private SideInventoryManager manager;
        [SerializeField] private RectTransform content;
        [SerializeField] private SideItemSlotView slotPrefab;
        
        private readonly List<SideItemSlotView> _slots = new(); //캐싱

        private void OnEnable()
        {
            if (manager != null) manager.OnInventoryChanged += BuildSlots;
            BuildSlots();
        }

        private void OnDisable()
        {
            if (manager != null) manager.OnInventoryChanged -= BuildSlots;
        }

        private void BuildSlots()
        {
            if (manager == null || slotPrefab == null || content == null) return;
            
            ClearSlots();

            foreach (SideInventorySlotData slot in manager.GetAllItems())
            {
                SideItemSlotView view = Instantiate(slotPrefab, content);
                view.Initialize(slot.Item, slot.Count);
                _slots.Add(view);
            }
            
        }

        private void ClearSlots()
        {
            foreach (SideItemSlotView slot in _slots)
            {
                if (slot != null) Destroy(slot.gameObject);
            }
            _slots.Clear();
        }
    }
}