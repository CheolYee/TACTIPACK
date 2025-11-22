using System;
using System.Collections.Generic;
using _00.Work.Scripts.Managers;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.ItemTypes;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace _00.Work.WorkSpace.Soso7194._04.Scripts.UI
{
    public class ShopManager : MonoSingleton<ShopManager>
    {
        [Header("Shop UI")]
        [SerializeField] private Image shopManu;
        [SerializeField] private TextMeshProUGUI coinText;
        [SerializeField] private TextMeshProUGUI resetText;
        [SerializeField] private CanvasGroup canvasGroup;
        
        [Header("Shop Setting")]
        [SerializeField] private int coin;
        [SerializeField] private int resetCount = 3;
        
        [SerializeField] private AllItemDatabase item;
        
        [SerializeField] private List<ItemSlot> itemSlots;

        private int _resetCount;
        public int Coin
        {
            get => coin;
            set => coin = Mathf.Clamp(value, 0, int.MaxValue);
        }

        protected override void Awake()
        {
            base.Awake();
            _resetCount = resetCount;
            Coin = coin;
        }

        private void Start()
        {
            SetShop();
        }

        private void OnEnable()
        {
            ItemSlot.OnUICoinChanged += ChangeCoinText;
            
            coinText.text = Coin.ToString();
            
            Sequence seq = DOTween.Sequence();
            seq.Append(shopManu.rectTransform.DOMoveY(550, 0.5f));
        }
        
        private void OnDisable()
        {
            ItemSlot.OnUICoinChanged -= ChangeCoinText;
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
                resetText.text = _resetCount.ToString();
                Coin -= 100;
                ChangeCoinText();
                SetShop();
            }
            else
            {
                Debug.Log("리롤 횟수를 다 썼습니다!");
            }
        }

        public void SetShop()
        {
            int itemCount = itemSlots.Count;
            
            List<ItemDataSo> itemSet = new();

            while (itemSet.Count < itemCount)
            {
                int random = Random.Range(0, item.AllItems.Count);
                
                if (itemSet.Contains(item.AllItems[random])) continue;
                
                itemSet.Add(item.AllItems[random]);
                Debug.Log(itemSet.Count);
            }

            for (int i = 0; i < itemCount; i++)
            {
                ItemDataSo currentItem = itemSet[i];
                ItemSlot itemSlot = itemSlots[i];
                
                itemSlot.Initialize(currentItem);
            }
        }

        public void Exit()
        {
            Sequence seq = DOTween.Sequence();
            seq.Append(shopManu.rectTransform.DOMoveY(-550, 0.5f)).onComplete += () =>
            {
                canvasGroup.gameObject.SetActive(false);
            };
        }
    }
}
