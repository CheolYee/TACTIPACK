using System.Collections.Generic;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Creatures.Players;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Save
{
    [CreateAssetMenu(fileName = "PlayerDatabase", menuName = "SO/Character/PlayerDatabase", order = 0)]
    public class PlayerDatabaseSo : ScriptableObject
    {
        [SerializeField] private List<PlayerDefaultData> allPlayers = new();

        private Dictionary<int, PlayerDefaultData> _byId;

        private void OnEnable()
        {
            BuildLookup();
        }

        private void BuildLookup()
        {
            _byId = new Dictionary<int, PlayerDefaultData>();
            foreach (var player in allPlayers)
            {
                if (player == null) continue;
                int id = player.CharacterId;
                if (_byId.ContainsKey(id))
                {
                    Debug.LogWarning($"[PlayerDatabase] 중복 CharacterId: {id}");
                    continue;
                }

                _byId.Add(id, player);
            }
        }

        public PlayerDefaultData GetById(int id)
        {
            if (_byId == null || _byId.Count == 0)
                BuildLookup();

            return _byId != null && _byId.TryGetValue(id, out var so) ? so : null;
        }
    }
}