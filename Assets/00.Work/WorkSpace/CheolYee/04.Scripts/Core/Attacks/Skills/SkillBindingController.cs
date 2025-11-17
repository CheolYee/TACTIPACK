using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.ItemTypes.ActiveItems;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.UI;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Creatures.Players;
using _00.Work.WorkSpace.CheolYee._04.Scripts.UI.Turn;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Attacks.Skills
{
    public class SkillBindingController : MonoBehaviour
    {
        [Header("References")] 
        [SerializeField] private GridInventoryUIController gridUi; //그리드 컨트롤러
        [SerializeField] private ItemDatabase itemDatabase; //아이템 DB
        [SerializeField] private GameObject bindingOverlay; //검은 배경
        [SerializeField] private Button cancelBindingButton; //바인드 해제버튼
        [SerializeField] private CanvasGroup errorCanvasGroup; //에러 표시용
        
        [Header("Error Visual")]
        [SerializeField] private TextMeshProUGUI errorText;
        [SerializeField] private float errorFadeDuration = 0.2f;
        [SerializeField] private float errorDisplayDuration = 1f;
        
        [Header("Turn UI Roots")]
        [SerializeField] private RectTransform turnUiRoot;
        
        [Header("Highlight Order")]
        [SerializeField] private int bindingHighlightOrder = 1200;
        
        private TurnSkillSlotUi _currentSkillSlot; //현재 바인드중인 스킬슬롯UI
        private TurnSlotUi _currentTurnSlot; //현재 바인드중인 턴 슬롯 UI
        private Player _currentPlayer; //현재 바인드 중인 캐릭터
        private bool _isBinding; //바인딩 되었는가
        
        private void Start()
        {
            SetOverlayActive(false);
            
            if (cancelBindingButton != null)
                cancelBindingButton.onClick.AddListener(CancelBinding);

            if (gridUi != null)
                gridUi.OnItemReturnedToSideInventory += HandleItemReturnedToSideInventory;
        }

        private void HandleItemReturnedToSideInventory(ItemInstance item)
        {
            if (item == null || turnUiRoot == null) return;
            
            TurnSkillSlotUi[] slots = turnUiRoot.GetComponentsInChildren<TurnSkillSlotUi>(true);

            foreach (TurnSkillSlotUi slot in slots)
            {
                if (slot.BoundItemInstance == item)
                {
                    //이 아이템을 바인딩하던 슬롯의 바인딩 해제
                    slot.ClearBinding();

                    //이 슬롯을 대상으로 바인딩 모드였으면 바인딩 모드도 정리
                    if (_isBinding && _currentSkillSlot == slot)
                    {
                        CancelBinding();
                    }
                }
            }
        }

        private void OnDestroy()
        {
            //이벤트 해제
            if (cancelBindingButton != null)
                cancelBindingButton.onClick.RemoveListener(CancelBinding);
            
            if (gridUi != null)
                gridUi.OnItemReturnedToSideInventory -= HandleItemReturnedToSideInventory;
        }

        public void RegisterSkillSlot(TurnSkillSlotUi slot)
        {
            if (slot == null) return;
            slot.OnRequestBind += BeginBindingFromSlot;
        }

        //슬롯에서 바인딩 버튼이 눌렸을 때 호출
        private void BeginBindingFromSlot(TurnSkillSlotUi slot)
        {
            if (slot == null || slot.Owner == null) return;

            if (_isBinding)
            {
                CancelBinding();
            }
            
            _currentSkillSlot = slot;
            _currentPlayer = slot.Owner;
            
            // 부모 TurnSlotUi 가져오기
            _currentTurnSlot = slot.GetComponentInParent<TurnSlotUi>();
            
            _isBinding = true;

            SetTurnSlotRayCast(false);
            
            _currentTurnSlot.EnableBindingHighlight(bindingHighlightOrder);
            
            SetOverlayActive(true);

            if (gridUi != null)
            {
                //바인드 진입요청
                gridUi.EnterBindingMode(OnGridItemSelectedForBinding);
            }
            
            HideErrorImmediate();
        }

        private void SetTurnSlotRayCast(bool enable)
        {
            if (turnUiRoot == null) return;

            TurnSlotUi[] slots = turnUiRoot.GetComponentsInChildren<TurnSlotUi>(true);
            foreach (var slot in slots)
            {
                var cg = slot.GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    cg.blocksRaycasts = enable;
                }
            }
        }

        //그리드에서 어떤 셀을 클릭했는가
        private void OnGridItemSelectedForBinding(ItemInstance inst)
        {
            if (!_isBinding || _currentSkillSlot == null || _currentPlayer == null ) return;

            if (inst == null)
            {
                //빈칸 클릭 시 바인드 지우고 종료
                if (_currentSkillSlot.BoundSkill != null)
                    _currentSkillSlot.ClearBinding();
                
                CancelBinding();
                return;
            }

            if (itemDatabase == null)
            {
                Debug.LogError("ItemDatabase가 할당되어 있지 않습니다.");
                ShowError("내부 오류: ItemDatabase 미할당");
                return;
            }

            // 인스턴스로부터 SO 데이터 가져오기
            ItemDataSo data = itemDatabase.GetItemById(inst.dataId);
            if (data == null)
            {
                ShowError("알 수 없는 아이템입니다.");
                return;
            }

            //공격 아이템인지 검사
            if (data is not AttackItemSo attackItem)
            {
                ShowError("공격 스킬만 바인딩할 수 있습니다.");
                return;
            }

            //래스 매칭 검사
            if (!IsClassCompatible(_currentPlayer, attackItem))
            {
                ShowError("이 캐릭터는 이 스킬을 사용할 수 없습니다.");
                return;
            }
            
            if (IsAlreadyBoundToOtherSlot(inst, _currentSkillSlot))
            {
                ShowError("이미 다른 캐릭터의 슬롯에 바인딩된 아이템입니다.");
                return;
            }

            //쿨타임 검사
            if (IsOnCooldown(_currentPlayer, attackItem))
            {
                ShowError("이 스킬은 현재 쿨타임입니다.");
                return;
            }

            //모든 조건을 통과했으면 바인딩 실행
            _currentSkillSlot.SetBoundSkill(attackItem, inst);

            //바인딩 완료 후 모드 종료
            CancelBinding();
        }

        
        //선택한 아이템과 플레이어의 직업이 맞거나 공용인지 확인
        private bool IsClassCompatible(Player player, ItemDataSo item)
        {
            if (item == null || player == null || player.CharacterData == null)
                return false;
            
            //공용은 그냥 허용
            if (item.itemClass == ItemClass.Any) return true;
            
            CharacterClass characterClass = player.CharacterData.CharacterClass;
            
            ItemClass playerItemClass = (ItemClass)(int)characterClass; //이넘값을 미리 정해서 캐스팅해서 변환
            return item.itemClass == playerItemClass;
        }
        
        //아이템이 쿨타임인가
        private bool IsOnCooldown(Player owner, AttackItemSo skill)
        {
            //쿨타임 아직없음
            return false;
        }

        private void ShowError(string message)
        {
            if (errorText == null)
            {
                Debug.LogError(message);
                return;
            }
            
            errorText.text = message;

            if (errorCanvasGroup != null)
            {
                errorCanvasGroup.gameObject.SetActive(true);
                errorCanvasGroup.DOKill();
                errorCanvasGroup.alpha = 0;
                
                Sequence sequence = DOTween.Sequence();
                sequence.Append(errorCanvasGroup.DOFade(1f, errorFadeDuration));
                sequence.AppendInterval(errorDisplayDuration);
                sequence.Append(errorCanvasGroup.DOFade(0f, errorFadeDuration));
                sequence.OnComplete(() =>
                {
                    errorCanvasGroup.gameObject.SetActive(false);
                });
            }
            else
            {
                errorText.gameObject.SetActive(true);
            }
        }

        //바인딩을 해제한다
        private void CancelBinding()
        {
            if (!_isBinding) return;
            
            _isBinding = false;
            
            SetTurnSlotRayCast(true);
            
            if (_currentTurnSlot != null)
                _currentTurnSlot.DisableBindingHighlight();
            
            _currentTurnSlot = null;
            _currentSkillSlot = null;
            _currentPlayer = null;
            
            //그리드 모드도 변경
            if (gridUi != null) gridUi.ExitBindingMode();
            
            SetOverlayActive(false);
            HideErrorImmediate(); //즉시 에러메시지들 숨기기
        }

        
        //즉시 에러메시지들을 숨기기
        private void HideErrorImmediate()
        {
            if (errorCanvasGroup != null)
            {
                errorCanvasGroup.DOKill();
                errorCanvasGroup.alpha = 0;
                errorCanvasGroup.gameObject.SetActive(false);
            }
        }


        //눈에 보이는 비주얼 껐다켰다
        private void SetOverlayActive(bool active)
        {
            if (bindingOverlay != null)
                bindingOverlay.SetActive(active);
            
            if (cancelBindingButton != null)
                cancelBindingButton.gameObject.SetActive(active);
            
            
        }
        
        //다른곳에서 아이템 인스턴스를 바인딩하고 있는지 확인
        private bool IsAlreadyBoundToOtherSlot(ItemInstance inst, TurnSkillSlotUi currentSlot)
        {
            if (turnUiRoot == null || inst == null) return false;

            TurnSkillSlotUi[] slots = turnUiRoot.GetComponentsInChildren<TurnSkillSlotUi>(true);
            foreach (var slot in slots)
            {
                //자기 자신은 제외
                if (slot == null || slot == currentSlot) continue;
        
                if (slot.BoundItemInstance == inst)
                    return true;
            }

            return false;
        }
    }
}