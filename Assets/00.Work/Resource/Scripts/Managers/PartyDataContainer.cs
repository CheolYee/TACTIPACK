using System.Collections.Generic;
using _00.Work.Scripts.Managers;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Creatures.Players;
using UnityEngine;

namespace _00.Work.Resource.Scripts.Managers
{
    public class PartyDataContainer : MonoSingleton<PartyDataContainer>
    {
        [SerializeField] private List<PlayerDefaultData> selectedParty = new();
        public IReadOnlyList<PlayerDefaultData> SelectedParty => selectedParty;

        protected override void Awake()
        {
            base.Awake();
            if (Instance == this)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        public void SetParty(IEnumerable<PlayerDefaultData> party)
        {
            selectedParty.Clear();
            if (party == null) return;

            foreach (var p in party)
            {
                if (p != null)
                    selectedParty.Add(p);
            }

            Debug.Log($"[PartyDataContainer] SetParty 호출: {selectedParty.Count}명 세팅됨");
        }
    }
}