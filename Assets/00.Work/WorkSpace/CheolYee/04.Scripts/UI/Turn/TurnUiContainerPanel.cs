using System;
using System.Collections.Generic;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Creatures.Players;
using UnityEngine;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Managers;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.UI.Turn
{
    public class TurnUiContainerPanel : MonoBehaviour
    {
        [Header("References")] 
        [SerializeField] private RectTransform content;
        [SerializeField] private TurnSlotUi slotPrefab;
        
        [Header("Limit")]
        [SerializeField] private int maxPlayers = 3;
        
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
            
            foreach (Player p in players)
            {
                TurnSlotUi slot = Instantiate(slotPrefab, content);
                slot.name = $"[Turn UI Slot] : {p.name}";
                slot.BindPlayer(p);
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
    }
}