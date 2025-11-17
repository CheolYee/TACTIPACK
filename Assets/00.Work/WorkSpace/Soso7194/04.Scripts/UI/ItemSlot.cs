using System;
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

        private void OnEnable()
        {
            if (ShopManager.Instance != null)
                ShopManager.Instance.OnItemChanged += OnChangeItem;
        }

        private void OnDisable()
        {
            if (ShopManager.Instance != null)
                ShopManager.Instance.OnItemChanged -= OnChangeItem;
        }

        
        private void Start()
        {
            _buttonText = itemButton.GetComponentInChildren<TextMeshProUGUI>();
            _coin = Random.Range(1, 9) * 10;
            _buttonText.text = _coin.ToString();
            // SO 값 받아오기
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

        private void OnChangeItem()
        {
            Debug.Log("아이템 교체");
            _coin = Random.Range(1, 9) * 10;
            _buttonText.text = _coin.ToString();
            _isSell = false;
        }
    }
}