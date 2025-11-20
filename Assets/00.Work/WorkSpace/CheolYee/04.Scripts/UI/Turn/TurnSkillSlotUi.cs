using System;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.ItemTypes.ActiveItems;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Creatures.Players;
using UnityEngine;
using UnityEngine.UI;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.UI.Turn
{
    public class TurnSkillSlotUi : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image iconImage; //스킬이 들어왔을 때 보여줄 아이콘 이미지
        [SerializeField] private GameObject emptyFrame; //비어있을 때 보여줄 머시기
        [SerializeField] private Button bindButton; //바인딩 버튼 (클릭 시 바인드 모드 전환)
        
        public Player Owner { get; private set; } //이 스킬을 쓸 플레이어
        public AttackItemSo BoundSkill { get; private set; } //바인딩된 스킬 데이터
        public ItemInstance BoundItemInstance { get; private set; } //인벤 위에 올려져있는 아이템 인스턴스

        public event Action<TurnSkillSlotUi> OnRequestBind; //바인딩 시작 알리기
        
        private bool _locked; //슬롯 잠금 여부 확인
        public bool IsLocked => _locked;

        private void Awake()
        {
            if (bindButton != null)
                bindButton.onClick.AddListener(HandleBindButtonClicked);

            RefreshVisual();
        }
        
        public void Initialize(Player owner) => Owner = owner;
        
        //공격 스킬 바인딩
        public void SetBoundSkill(AttackItemSo skill, ItemInstance inst = null)
        {
            if (_locked && skill != null)
            {
                Debug.Log("슬롯이 잠겨 있어 스킬 바인딩을 무시합니다.");
                return;
            }
            
            BoundSkill = skill;
            BoundItemInstance = inst;

            RefreshVisual();
        }
        
        //슬롯을 잠궈요
        public void SetLocked(bool locked)
        {
            if (_locked == locked)
                return;

            _locked = locked;

            //버튼 입력 막기
            if (bindButton != null)
                bindButton.interactable = !locked;
        }

        //바인딩을 제거한다
        public void ClearBinding()
        {
            BoundSkill = null;
            BoundItemInstance = null;
            RefreshVisual();
        }

        //비주얼 재설정
        private void RefreshVisual()
        {
            bool hasSkill = BoundSkill != null;
            
            if (emptyFrame != null) emptyFrame.SetActive(!hasSkill);

            if (iconImage != null)
            {
                iconImage.enabled = hasSkill;
                //아이콘 있으면 적용 없으면 비우기
                iconImage.sprite =
                    hasSkill ? BoundSkill.icon != null ? BoundSkill.icon : null : null;
            }
        }

        public void HandleBindButtonClicked()
        {
            if (_locked)
            {
                Debug.Log("슬롯이 잠겨있어 바인드를 무시했습니다.");
                return;
            }
            
            OnRequestBind?.Invoke(this);
        }
    }
}