using System;
using _00.Work.WorkSpace.CheolYee._04.Scripts.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.UI.SideItem
{
    public class SideItemSlotView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("References")] 
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI countText;
        [SerializeField] private GameObject highLight;
        [SerializeField] private TooltipTarget _tooltip;
        
        private SideInventoryManager _sideManager; //인스펙터 연결 권장
        private GridInventoryUIController _gridUI;
        
        public ItemDataSo Data { get; private set; }
        public int Count { get; private set; }
        public Sprite Icon { get => icon.sprite; set => icon.sprite = value; }

        private void Start()
        {
            _sideManager = SideInventoryManager.Instance;
            _gridUI = GridInventoryUIController.Instance;
        }

        public void Initialize(ItemDataSo data, int count)
        {
            Data = data;
            icon.sprite = data != null ? data.icon : null;
            SetCount(count);

            if (_tooltip != null && Data != null)
            {
                _tooltip.SetText(
                    Data.itemName,
                    Data.description      // 여기에 리치텍스트 써도 됨
                );
            }
        }

        private void SetCount(int count)
        {
            Count = Mathf.Max(0, count);
            countText.text = Count >= 1 ? Count.ToString() : "0";
        }

        public void SetHighlight(bool highlight)
        {
            highLight.SetActive(highlight);
        }
        
        public void BeginDragFromSide(ItemDataSo so, SideInventoryManager sideManager, GridInventoryUIController gridUI)
        {
            //1개 보류 차감(실패 시 환불 예정)
            int removed = sideManager.RemoveItem(so);
            if (removed <= 0) return; //아이템 없음

            //그리드 컨트롤러에 드래그 시작시키기
            gridUI.StartDragFromSide(so, sideManager);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (Data == null) return;
            BeginDragFromSide(Data, _sideManager, _gridUI);
        }

        public void OnDrag(PointerEventData eventData)
        {
        }

        public void OnEndDrag(PointerEventData eventData)
        {
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            SetHighlight(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            SetHighlight(false);
        }
    }
}