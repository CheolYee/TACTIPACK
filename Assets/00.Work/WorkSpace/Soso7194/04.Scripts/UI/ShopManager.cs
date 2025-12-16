using System.Collections.Generic;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.ItemTypes;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Managers;
using _00.Work.WorkSpace.JaeHun._01._Scrpits;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _00.Work.WorkSpace.Soso7194._04.Scripts.UI
{
    public class ShopManager : MonoBehaviour
    {
        [Header("Shop UI")]
        [SerializeField] private RectTransform shopMenuRect;
        [SerializeField] private TextMeshProUGUI coinText;
        [SerializeField] private TextMeshProUGUI resetText;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Character")]
        [SerializeField] private RectTransform characterRect;
        [SerializeField] private float characterSlideOffsetX = 600f;

        [Header("Shop Setting")]
        [SerializeField] private int resetCount = 3;

        [SerializeField] private AllItemDatabase item;
        [SerializeField] private List<ItemSlot> itemSlots;

        private int _resetCount;

        // 캐릭터의 원래 위치 저장
        private Vector2 _characterOriginalPos;
        private bool _characterPosInitialized;
        
        //샵 패널의 원래 위치 저장
        private Vector2 _shopMenuOriginalPos;

        private void Awake()
        {
            _resetCount = resetCount;

            if (characterRect != null)
            {
                _characterOriginalPos = characterRect.anchoredPosition;
                _characterPosInitialized = true;
            }
        }

        private void OnEnable()
        {
            ItemSlot.OnUICoinChanged += ChangeCoinText;

            _resetCount = resetCount;
            SetShop();

            if (MoneyManager.Instance != null)
            {
                MoneyManager.Instance.OnCoinChanged += OnCoinChanged;
            }

            ChangeCoinText();

            if (canvasGroup != null)
            {
                canvasGroup.gameObject.SetActive(true);
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }

            Sequence seq = DOTween.Sequence();

            if (shopMenuRect != null)
            {
                var rt = shopMenuRect;

                // 기준점(원래 자리)
                _shopMenuOriginalPos = rt.anchoredPosition;

                // 아래에서 시작
                Vector2 startPos = _shopMenuOriginalPos;
                startPos.y = -550f;                // 캔버스 기준 픽셀 단위
                rt.anchoredPosition = startPos;

                // 원래 자리까지 올라오기
                seq.Append(rt.DOAnchorPosY(_shopMenuOriginalPos.y, 0.5f)
                    .SetEase(Ease.OutCubic));
            }

            if (characterRect != null)
            {
                if (!_characterPosInitialized)
                {
                    _characterOriginalPos = characterRect.anchoredPosition;
                    _characterPosInitialized = true;
                }

                Vector2 fromPos = _characterOriginalPos + Vector2.left * characterSlideOffsetX;
                characterRect.anchoredPosition = fromPos;

                seq.Join(characterRect.DOAnchorPos(_characterOriginalPos, 0.5f)
                    .SetEase(Ease.OutCubic));
            }
        }

        private void OnDisable()
        {
            ItemSlot.OnUICoinChanged -= ChangeCoinText;

            if (MoneyManager.Instance != null)
            {
                MoneyManager.Instance.OnCoinChanged -= OnCoinChanged;
            }
        }

        private void OnCoinChanged(int newValue)
        {
            coinText.text = newValue.ToString();
        }

        private void ChangeCoinText()
        {
            int current = MoneyManager.Instance != null ? MoneyManager.Instance.Coin : 0;
            coinText.text = current.ToString();
        }

        public void Reroll()
        {
            if (_resetCount <= 0)
            {
                Debug.Log("리롤 횟수를 다 썼습니다!");
                return;
            }

            var money = MoneyManager.Instance;
            if (money == null)
            {
                Debug.LogWarning("[ShopManager] MoneyManager 가 없습니다.");
                return;
            }

            const int rerollCost = 50;

            if (!money.TrySpend(rerollCost))
            {
                Debug.Log("골드가 부족합니다!");
                return;
            }

            _resetCount--;
            if (resetText != null)
                resetText.text = _resetCount.ToString();

            ChangeCoinText();
            SetShop();
        }

        private void SetShop()
        {
            
            int itemCount = itemSlots.Count;

            List<ItemDataSo> itemSet = new();

            while (itemSet.Count < itemCount)
            {
                int random = Random.Range(0, item.AllItems.Count);

                if (itemSet.Contains(item.AllItems[random])) continue;

                itemSet.Add(item.AllItems[random]);
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
            if (!canvasGroup.interactable)
                return;

            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            var seq = DOTween.Sequence();

            // 패널: 위에 있던 걸 아래로 내리기
            if (shopMenuRect != null)
            {
                var rt = shopMenuRect;

                float targetY = -550f;   // 화면 아래로
                seq.Append(rt.DOAnchorPosY(targetY, 0.5f)
                    .SetEase(Ease.InCubic));
            }

            // 캐릭터: 왼쪽으로 빠지게
            if (characterRect != null)
            {
                if (!_characterPosInitialized)
                {
                    _characterOriginalPos = characterRect.anchoredPosition;
                    _characterPosInitialized = true;
                }

                Vector2 toPos = _characterOriginalPos + Vector2.left * characterSlideOffsetX;

                seq.Join(characterRect.DOAnchorPos(toPos, 0.5f)
                    .SetEase(Ease.InCubic));
            }

            seq.OnComplete(() =>
            {
                if (canvasGroup != null)
                {
                    canvasGroup.gameObject.SetActive(false);
                }

                var mapMgr = MapManager.Instance;
                if (mapMgr != null)
                {
                    mapMgr.CompleteCurrentMap();
                    mapMgr.ShowCurrentChapterRoot();
                }
            });
        }
    }
}
