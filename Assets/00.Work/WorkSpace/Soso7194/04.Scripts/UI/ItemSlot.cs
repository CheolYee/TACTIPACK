using System;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.UI.SideItem;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Managers;
using _00.Work.WorkSpace.CheolYee._04.Scripts.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _00.Work.WorkSpace.Soso7194._04.Scripts.UI
{
    public class ItemSlot : MonoBehaviour
    {
        [Header("Item Slot")]
        [SerializeField] private Image itemImage;
        [SerializeField] private TextMeshProUGUI itemName;
        [SerializeField] private Button itemButton;
        [SerializeField] private TooltipTarget tooltipTarget;
        
        public static Action OnUICoinChanged;
        
        private int _coin = 10;
        private TextMeshProUGUI _buttonText;
        private bool _isSell;

        private ItemDataSo _currentItem;
        
        private void Awake()
        {
            _buttonText = itemButton.GetComponentInChildren<TextMeshProUGUI>();
        }

        public void Initialize(ItemDataSo itemData)
        {
            _currentItem = itemData;
            itemImage.sprite = itemData.icon;
            itemName.text = itemData.itemName;
            _coin = itemData.price;
            _isSell = false;
            
            _buttonText.text = $"{_coin.ToString()}$";
            tooltipTarget.SetText(itemData.itemName, itemData.description);
        }
        
        public void OnClickButton()
        {
            if (MoneyManager.Instance.Coin > _coin && !_isSell) // 가지고 있는 코인보다 많으면
            {
                _buttonText.text = "매진";
                MoneyManager.Instance.TrySpend(_coin);
                SideInventoryManager.Instance.AddItem(_currentItem);
                _isSell = true;
                OnUICoinChanged?.Invoke();
            }
            else
            {
                Debug.Log("이미 팔렸거나 돈이 없습니다!");
            }
        }
    }
}