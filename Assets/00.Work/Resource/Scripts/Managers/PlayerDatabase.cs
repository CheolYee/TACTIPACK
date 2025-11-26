using System.Collections.Generic;
using _00.Work.Scripts.Managers;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Creatures.Players;
using UnityEngine;

namespace _00.Work.Resource.Scripts.Managers
{
    public class PlayerDatabase : MonoSingleton<PlayerDatabase>
    {
        [Header("All Player Datas")]
        [SerializeField] private List<PlayerDefaultData> allPlayers = new();

        // ID → SO 빠른 조회용
        private readonly Dictionary<int, PlayerDefaultData> _byId = new();

        public IReadOnlyList<PlayerDefaultData> AllPlayers => allPlayers;

        protected override void Awake()
        {
            base.Awake();
            if (Instance == this)
            {
                DontDestroyOnLoad(gameObject);
                BuildLookup();
            }
        }

        private void BuildLookup()
        {
            _byId.Clear();

            foreach (var data in allPlayers)
            {
                if (data == null) continue;

                int id = data.CharacterId;
                if (_byId.ContainsKey(id))
                {
                    Debug.LogWarning($"[PlayerDatabase] 중복 CharacterId 감지: {id} ({data.CharacterName})");
                    continue;
                }

                _byId.Add(id, data);
            }
        }

        /// <summary>
        /// CharacterId로 PlayerDefaultData 찾기
        /// </summary>
        public PlayerDefaultData GetById(int id)
        {
            if (_byId.TryGetValue(id, out var data))
                return data;

            Debug.LogWarning($"[PlayerDatabase] CharacterId {id} 에 해당하는 데이터가 없습니다.");
            return null;
        }

        /// <summary>
        /// 직업(Class)으로 하나 찾기 (필요하면 사용)
        /// </summary>
        public PlayerDefaultData GetByClass(CharacterClass characterClass)
        {
            foreach (var data in allPlayers)
            {
                if (data != null && data.CharacterClass == characterClass)
                    return data;
            }

            return null;
        }
    }
}
