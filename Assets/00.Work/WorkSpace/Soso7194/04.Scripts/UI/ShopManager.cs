using System.Collections.Generic;
using _00.Work.Scripts.Managers;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.ItemTypes;
using _00.Work.WorkSpace.JaeHun._01._Scrpits;
using DG.Tweening;
using TMPro;
using UnityEngine;
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

        [Header("Character")]
        [SerializeField] private RectTransform characterRect;
        [SerializeField] private float characterSlideOffsetX = 600f;

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

        // 캐릭터의 원래 위치 저장
        private Vector3 _characterOriginalPos;
        private bool _characterPosInitialized;

        protected override void Awake()
        {
            base.Awake();
            _resetCount = resetCount;
            Coin = coin;

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

            coinText.text = Coin.ToString();

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

            // 캐릭터: 오른쪽으로 빠지게
            if (characterRect != null)
            {
                if (!_characterPosInitialized)
                {
                    _characterOriginalPos = characterRect.position;
                    _characterPosInitialized = true;
                }

                Vector3 toPos = _characterOriginalPos + Vector3.right * characterSlideOffsetX;

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
                    mapMgr.CompleteCurrentMap();          // 다음 노드 해금
                    mapMgr.ShowCurrentChapterRoot();  // 맵 UI 다시 위로 등장
                }
            });
        }
    }
}
