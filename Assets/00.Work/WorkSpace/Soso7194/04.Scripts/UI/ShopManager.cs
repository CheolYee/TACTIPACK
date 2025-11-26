using System.Collections.Generic;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.ItemTypes;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Managers;
using _00.Work.WorkSpace.JaeHun._01._Scrpits;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace _00.Work.WorkSpace.Soso7194._04.Scripts.UI
{
    public class ShopManager : MonoBehaviour
    {
        [Header("Shop UI")]
        [SerializeField] private Image shopManu;
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

        public int Coin
        {
            get => MoneyManager.Instance != null ? MoneyManager.Instance.Coin : 0;
            set
            {
                if (MoneyManager.Instance != null)
                    MoneyManager.Instance.SetCoin(value);
            }
        }

        // 캐릭터의 원래 위치 저장
        private Vector3 _characterOriginalPos;
        private bool _characterPosInitialized;
        

        private void Awake()
        {
            _resetCount = resetCount;

            if (characterRect != null)
            {
                _characterOriginalPos = characterRect.position;
                _characterPosInitialized = true;
            }
        }

        private void Start()
        {
            SetShop();
        }

        private void OnEnable()
        {
            ItemSlot.OnUICoinChanged += ChangeCoinText;

            // MoneyManager 이벤트도 구독해서 어디서든 코인 바뀌면 텍스트 갱신 ★
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

            //등장 트윈
            Sequence seq = DOTween.Sequence();

            if (shopManu != null)
            {
                var rt = shopManu.rectTransform;

                var startPos = rt.position;
                startPos.y = -550f;
                rt.position = startPos;

                seq.Append(rt.DOMoveY(550f, 0.5f)
                    .SetEase(Ease.OutCubic));
            }
            
            if (characterRect != null)
            {
                if (!_characterPosInitialized)
                {
                    _characterOriginalPos = characterRect.position;
                    _characterPosInitialized = true;
                }

                Vector3 fromPos = _characterOriginalPos + Vector3.left * characterSlideOffsetX;
                characterRect.position = fromPos;

                seq.Join(characterRect.DOMove(_characterOriginalPos, 0.5f)
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
            if (shopManu != null)
            {
                var rt = shopManu.rectTransform;
                seq.Append(rt.DOMoveY(-550f, 0.5f)
                    .SetEase(Ease.InCubic));
            }

            // 캐릭터: 오른쪽/왼쪽으로 빠지게 (원래 코드 유지)
            if (characterRect != null)
            {
                if (!_characterPosInitialized)
                {
                    _characterOriginalPos = characterRect.position;
                    _characterPosInitialized = true;
                }

                Vector3 toPos = _characterOriginalPos + Vector3.left * characterSlideOffsetX;

                // 패널과 동시에 나가도록 Join
                seq.Join(characterRect.DOMove(toPos, 0.5f)
                    .SetEase(Ease.InCubic));
            }

            seq.OnComplete(() =>
            {
                // 상점 UI 비활성화
                if (canvasGroup != null)
                {
                    canvasGroup.gameObject.SetActive(false);
                }

                // 1) 현재 맵(상점 노드) 클리어 처리
                var mapMgr = MapManager.Instance;
                if (mapMgr != null)
                {
                    mapMgr.CompleteCurrentMap();      // 다음 노드 해금
                    mapMgr.ShowCurrentChapterRoot();  // 맵 UI 다시 위로 등장
                }
            });
        }
    }
}
