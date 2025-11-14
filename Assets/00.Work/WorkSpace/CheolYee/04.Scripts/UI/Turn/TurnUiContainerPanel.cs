using System;
using System.Collections.Generic;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Attacks.Skills;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Creatures.Players;
using UnityEngine;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Managers;
using DG.Tweening;
using UnityEngine.UI;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.UI.Turn
{
    public class TurnUiContainerPanel : MonoBehaviour
    {
        [Header("References")] 
        [SerializeField] private SkillBindingController skillBindingController;
        [SerializeField] private RectTransform content;
        [SerializeField] private TurnSlotUi slotPrefab; //실제 드래그되는 슬롯
        [SerializeField] private Canvas canvas;
        [SerializeField] private VerticalLayoutGroup layoutGroup;
        
        [Header("Limit")]
        [SerializeField] private int maxPlayers = 3;
        
        public VerticalLayoutGroup LayoutGroup => layoutGroup;
        public RectTransform Content => content;
        
        private readonly List<TurnSlotUi> _slots = new();

        private void Awake()
        {
            Build();
        }

        private void Build()
        {
            if (slotPrefab == null || content == null) return;

            List<Player> players = PartyManager.Instance.Players;
            if (players == null || players.Count == 0) return;
            
            int count = Math.Min(maxPlayers, players.Count);


            for (int i = 0; i < count; i++)
            {
                Player p = players[i];
                TurnSlotUi slot = Instantiate(slotPrefab, content);
                slot.name = $"[Turn UI Slot] : {p.CharacterData.name}";
                
                slot.Initialize(this, canvas);
                slot.BindPlayer(p);
                
                var skillSlot = slot.GetSkillSlot();
                if (skillSlot != null)
                {
                    skillBindingController.RegisterSkillSlot(skillSlot);
                }
                
                _slots.Add(slot);
            }
        }

        private void Clear()
        { 
            foreach (var s in _slots)
            {
                if (s != null) Destroy(s.gameObject);
            }
            _slots.Clear();
        }
        public List<Player> GetCurrentOrder()
        {
            List<Player> order = new List<Player>(_slots.Count);
            foreach (var s in _slots)
            {
                order.Add(s.BoundPlayer);
            }
            return order;
        }
        
        public void SwapSlots(TurnSlotUi a, TurnSlotUi b)
        {
            if (a == null || b == null || a == b) return;

            int indexA = _slots.IndexOf(a);
            int indexB = _slots.IndexOf(b);

            if (indexA == -1 || indexB == -1 || indexA == indexB)
                return;

            //리스트 상에서 순서 교체
            (_slots[indexA], _slots[indexB]) = (_slots[indexB], _slots[indexA]);

            //리스트에서 교체된거 현실반영
            for (int i = 0; i < _slots.Count; i++)
            {
                _slots[i].transform.SetSiblingIndex(i);
            }

            PlaySwapAnimation(a.transform, b.transform);
        }
        
        private void PlaySwapAnimation(Transform a, Transform b)
        {
            float upScale = 1.1f;
            float downScale = 0.9f;
            float duration = 0.12f;

            Vector3 originalScaleA = a.localScale;
            Vector3 originalScaleB = b.localScale;

            Sequence seq = DOTween.Sequence();

            seq.Join(a.DOScale(originalScaleA * upScale, duration));
            seq.Join(b.DOScale(originalScaleB * downScale, duration));

            seq.Append(a.DOScale(originalScaleA, duration));
            seq.Join(b.DOScale(originalScaleB, duration));
        }
    }
}