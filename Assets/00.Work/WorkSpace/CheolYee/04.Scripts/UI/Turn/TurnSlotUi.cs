using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Effects;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Events;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.ItemTypes.ActiveItems;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Creatures.Players;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.UI.Turn
{
    public class TurnSlotUi : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
    {
        [Header("References")]
        [SerializeField] private Image icon;
        [SerializeField] private TurnSkillSlotUi skillSlot;
        [SerializeField] private float dragThreshold = 10f;
        
        [Header("Sorting")]
        [SerializeField] private Canvas slotCanvas;
        [SerializeField] private int dragSortingOrder = 1000; // 드래그 중일 때 순서
        public Player BoundPlayer {get; private set;}
        public bool DraggedEnough => _draggedEnough;
        
        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;
        private TurnUiContainerPanel _ownerPanel;
        private Canvas _canvas;
        
        private Vector2 _startPosition;
        private Vector2 _dragStartScreenPos;
        private bool _isDropped;
        private bool _draggedEnough;
        
        private bool _savedOverrideSorting;
        private int _savedSortingOrder;
        
        public static TurnSlotUi CurrentlyDragging {get; private set;}
        
        public TurnSkillSlotUi GetSkillSlot() => skillSlot;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            
            if (slotCanvas == null)
            {
                slotCanvas = GetComponent<Canvas>();
                if (slotCanvas == null)
                {
                    slotCanvas = gameObject.AddComponent<Canvas>();
                }
            }

            _savedOverrideSorting = slotCanvas.overrideSorting;
            _savedSortingOrder = slotCanvas.sortingOrder;

            //부모 Canvas 기준으로 동작하게
            slotCanvas.overrideSorting = _savedOverrideSorting;
            slotCanvas.sortingOrder = _savedSortingOrder;
        }

        public void Initialize(TurnUiContainerPanel owner, Canvas canvas)
        {
            _ownerPanel = owner;
            _canvas = canvas;
        }

        public void BindPlayer(Player player)
        {
            BoundPlayer = player;

            PlayerDefaultData data = player.CharacterData;
            if (data != null && icon != null)
                icon.sprite = data.CharacterIcon;
            
            if (skillSlot != null) skillSlot.Initialize(player);
        }

        public void ExecuteBoundSkill()
        {
            if (BoundPlayer == null)
            {
                Debug.LogWarning($"TurnSlotUi({name}): BoundPlayer가 없습니다.");
                Bus<SkillFinishedEvent>.Raise(new SkillFinishedEvent(BoundPlayer, null));
                return;
            }

            if (skillSlot == null)
            {
                Debug.LogWarning($"TurnSlotUi({name}): SkillSlot이 없습니다.");
                Bus<SkillFinishedEvent>.Raise(new SkillFinishedEvent(BoundPlayer, null));
                return;
            }

            AttackItemSo skill = skillSlot.BoundSkill;
            if (skill == null)
            {
                Debug.Log($"TurnSlotUi({name}): 바인딩된 스킬이 없어 공격을 실행하지 않습니다.");
                Bus<SkillFinishedEvent>.Raise(new SkillFinishedEvent(BoundPlayer, null));
                return;
            }
            
            BoundPlayer.Attack(skill);
            Debug.Log($"[TurnSlotUi] {BoundPlayer.CharacterData.name} 이(가) 스킬 {skill.name} 로 공격 실행");
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_ownerPanel == null || _canvas == null)
                return;

            CurrentlyDragging = this;
            _isDropped = false;

            _startPosition = _rectTransform.anchoredPosition;
            _dragStartScreenPos = eventData.position;
            _draggedEnough = false;
            
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_canvas == null) return;

            if (!_draggedEnough)
            {
                float dist = Vector2.Distance(_dragStartScreenPos, eventData.position);
                if (dist < dragThreshold) return;
                
                _draggedEnough = true;
                
                if (_canvasGroup != null)
                {
                    _canvasGroup.blocksRaycasts = false;
                    _canvasGroup.alpha = 0.8f;
                }

                if (_ownerPanel.LayoutGroup != null)
                    _ownerPanel.LayoutGroup.enabled = false;
                
                ApplyHighlightSorting(dragSortingOrder);
            }

            //부모를 기준으로 마우스 위치를 로컬 좌표로 변환
            RectTransform parentRect = _rectTransform.parent as RectTransform;
            if (parentRect == null) return;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parentRect,
                    eventData.position,
                    _canvas.worldCamera,
                    out Vector2 localPoint))
            {
                _rectTransform.anchoredPosition = localPoint;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            //많이 안움직임(거의 제자리라면) 그냥 유지
            if (!_draggedEnough)
            {
                _rectTransform.anchoredPosition = _startPosition;
                CurrentlyDragging = null;
                return;
            }
            
            // 다시 자기 슬롯이 레이캐스트를 받도록
            if (_canvasGroup != null)
            {
                _canvasGroup.blocksRaycasts = true;
                _canvasGroup.alpha = 1f;
            }
            
            //정렬복구
            RestoreSorting();

            //유효한 슬롯에 드롭되지 않았다면 원래 자리로 되돌리기
            if (!_isDropped)
            {
                _rectTransform.anchoredPosition = _startPosition;
            }

            //레이아웃 다시 활성화 후 재정렬
            if (_ownerPanel != null && _ownerPanel.LayoutGroup != null)
                _ownerPanel.LayoutGroup.enabled = true;

            CurrentlyDragging = null;
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (_ownerPanel == null) return;
            if (CurrentlyDragging == null) return;
            if (CurrentlyDragging == this) return; //자기 자신에게 드롭하는 경우 무시

            //진짜 드래그가 아니면 스왑 안해요
            if (!CurrentlyDragging.DraggedEnough) return;
            
            //드래그 중인 쪽에 드롭 성공 표시
            CurrentlyDragging._isDropped = true;

            //패널에 두 슬롯 스왑 요청
            _ownerPanel.SwapSlots(CurrentlyDragging, this);
        }
        
        private void SaveSorting()
        {
            if (slotCanvas == null) return;
            _savedOverrideSorting = slotCanvas.overrideSorting;
            _savedSortingOrder = slotCanvas.sortingOrder;
        }
        
        private void ApplyHighlightSorting(int order)
        {
            if (slotCanvas == null) return;
            slotCanvas.overrideSorting = true;
            slotCanvas.sortingOrder = order;
        }
        
        public void EnableBindingHighlight(int highlightOrder)
        {
            SaveSorting();
            slotCanvas.overrideSorting = true;
            slotCanvas.sortingOrder = highlightOrder;
        }
        
        public void DisableBindingHighlight()
        {
            RestoreSorting();
        }

        private void RestoreSorting()
        {
            if (slotCanvas == null) return;
            slotCanvas.overrideSorting = _savedOverrideSorting;
            slotCanvas.sortingOrder = _savedSortingOrder;
        }
    }
}