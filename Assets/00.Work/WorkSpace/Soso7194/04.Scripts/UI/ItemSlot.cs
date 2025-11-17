using System;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace _00.Work.WorkSpace.Soso7194._04.Scripts.UI
{
    public class ItemSlot : MonoBehaviour
    {
        [Header("Item Slot")]
        [SerializeField] private Image itemImage;
        [SerializeField] private TextMeshProUGUI itemName;
        [SerializeField] private Button itemButton;
        
        public static Action OnUICoinChanged;
        
        private int _coin = 10;
        private TextMeshProUGUI _buttonText;
        private bool _isSell = false;
        
        private void Awake()
        {
            _buttonText = itemButton.GetComponentInChildren<TextMeshProUGUI>();
        }

        public void Initialize(ItemDataSo itemData)
        {
            itemImage.sprite = itemData.icon;
            itemName.text = itemData.itemName;
            _coin = itemData.price;
            
            _buttonText.text = _coin.ToString();
        }
        
        public void OnClickButton()
        {
            if (ShopManager.Instance.Coin > _coin && !_isSell) // 가지고 있는 코인보다 많으면
            {
                Debug.Log("아이템을 샀습니다!");
                _buttonText.text = "Sold Out";
                ShopManager.Instance.Coin -= _coin;
                Debug.Log($"남은 골드 : {ShopManager.Instance.Coin}");
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