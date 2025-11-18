using System.Collections.Generic;
using _00.Work.Scripts.Managers;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Creatures.Players;
using _00.Work.WorkSpace.CheolYee._04.Scripts.UI.Turn;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Managers
{
    public class PartyManager : MonoSingleton<PartyManager>
    {
        [Header("Players")] 
        [SerializeField] private Player playerPrefab;
        [SerializeField] private List<Transform> playerSpawnPoints;

        [Header("RunTimeData")] 
        [SerializeField] private List<PlayerDefaultData> selectedPartyData = new();
        [SerializeField] private List<Player> players = new();
        public List<Player> Players => players;
        public List<PlayerDefaultData> SelectedPartyData => selectedPartyData;

        private bool _spawned;

        private void Start()
        {
            if (!_spawned && selectedPartyData.Count > 0)
            {
                SpawnPartyFromData(selectedPartyData);
            }
        }

        public void SetPartyAndSpawn(List<PlayerDefaultData> partyData)
        {
            selectedPartyData = partyData;
            SpawnPartyFromData(selectedPartyData);
        }

        private void SpawnPartyFromData(List<PlayerDefaultData> partyData)
        {
            if (playerPrefab == null)
            {
                Debug.LogError("[PartyManager] playerPrefab 이 설정되어 있지 않습니다.");
                return;
            }

            if (playerSpawnPoints == null || playerSpawnPoints.Count == 0)
            {
                Debug.LogError("[PartyManager] playerSpawnPoints 가 비어 있습니다.");
                return;
            }

            ClearParty();
            
            int count = Mathf.Min(partyData.Count, playerSpawnPoints.Count);
            for (int i = 0; i < count; i++)
            {
                var data = partyData[i];
                var spawnPoint = playerSpawnPoints[i];
                
                if (data == null || spawnPoint == null) continue;
                
                Player instance = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
                instance.name = $"[플레이어 {i}] {data.CharacterName}";

                instance.SetupCharacter(data);
                
                players.Add(instance);
            }
            _spawned = true;

            TurnUiContainerPanel.Instance.RebuildFromBattleManager();
        }

        private void ClearParty()
        {
            foreach (var p in players)
            {
                if (p != null)
                    Destroy(p.gameObject);
            }
            players.Clear();
        }
    }
}