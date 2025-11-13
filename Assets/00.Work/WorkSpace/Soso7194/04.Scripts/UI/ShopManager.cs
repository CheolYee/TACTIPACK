using System;
using _00.Work.Scripts.Managers;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace _00.Work.WorkSpace.Soso7194._04.Scripts.UI
{
    public class ShopManager : MonoBehaviour
    {
        public static ShopManager Instance { get; private set; }
        [SerializeField] private int coin;
        
        [SerializeField] private Image shopManu;
        [SerializeField] private int resetCount = 3;
        [SerializeField] private TextMeshProUGUI coinText;
        
        public Action OnItemChanged;
        
        private bool _isUp = false;
        private int _resetCount;
        public int Coin
        {
            get => coin;
            set => coin = Mathf.Clamp(value, 0, int.MaxValue);
        }
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            _resetCount = resetCount;
            Coin = coin;
        }
        
        private void OnEnable()
        {
            ItemSlot.OnCoinChanged += ChangeCoinText;
            
            coinText.text = Coin.ToString();
        }
        
        private void OnDisable()
        {
            ItemSlot.OnCoinChanged -= ChangeCoinText;
        }

        private void Update()
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                _isUp = !_isUp;
                Shop();
            }
        }

        private void Shop()
        {
            if (_isUp)
            {
                Sequence seq = DOTween.Sequence();
                seq.Append(shopManu.rectTransform.DOMoveY(550, 0.5f));
            }
            else
            {
                Sequence seq = DOTween.Sequence();
                seq.Append(shopManu.rectTransform.DOMoveY(-550, 0.5f));
            }
        }

        private void ChangeCoinText()
        {
            coinText.text = Coin.ToString();
        }

        public void Reroll()
        {
            if (_resetCount > 0)
            {
                _resetCount--;
                Coin -= 100;
                ChangeCoinText();
                OnItemChanged?.Invoke();
            }
            else
            {
                Debug.Log("리롤 횟수를 다 썼습니다!");
            }
        }

        public void Exit()
        {
            _isUp = !_isUp;
            Shop();
        }
    }
}
