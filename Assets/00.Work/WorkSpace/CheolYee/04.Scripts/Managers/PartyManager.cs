using System.Collections.Generic;
using _00.Work.Resource.Scripts.Managers;
using _00.Work.Scripts.Managers;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.UI.SideItem;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Creatures.Players;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Save;
using _00.Work.WorkSpace.CheolYee._04.Scripts.UI.Turn;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Managers
{
    public class PartyManager : MonoSingleton<PartyManager>
    {
        [Header("Players")] 
        [SerializeField] private Player playerPrefab;
        [SerializeField] private List<Transform> playerSpawnPoints;
        
        [Header("Databases")]
        [SerializeField] private PlayerDatabaseSo playerDatabase;

        [Header("RunTimeData")] 
        [SerializeField] private List<PlayerDefaultData> selectedPartyData = new();
        [SerializeField] private List<Player> players = new();
        public List<Player> Players => players;
        public List<PlayerDefaultData> SelectedPartyData => selectedPartyData;

        private bool _spawned;

        private void Start()
        {
            if (_spawned) return;
            
            var saveMgr = SaveManager.Instance;
            if (saveMgr != null)
            {
                var partySave = saveMgr.LoadParty();
                if (partySave is { players: { Count: > 0 } })
                {
                    ApplyPartyFromSave(partySave);
                    return;
                }
            }
            
            List<PlayerDefaultData> partyToSpawn = new();
            var container = PartyDataContainer.Instance;

            if (container != null &&
                container.SelectedParty is { Count: > 0 })
            {
                partyToSpawn = new List<PlayerDefaultData>(container.SelectedParty);
                Debug.Log($"[PartyManager] PartyDataContainer에서 {partyToSpawn.Count}명 파티 로드");
            }
            else if (selectedPartyData is { Count: > 0 })
            {
                // 컨테이너가 비어있으면 기존 인스펙터 데이터 사용 (에디터 테스트용)
                partyToSpawn = selectedPartyData;
                Debug.Log($"[PartyManager] 인스펙터 selectedPartyData로 {partyToSpawn.Count}명 파티 스폰");
            }
            
            if (!_spawned && selectedPartyData is { Count: > 0 })
            {
                SpawnPartyFromData(partyToSpawn);
                foreach (var data in partyToSpawn)
                {
                    SideInventoryManager.Instance.AddItem(data.StartItem);
                }
            }
        }

        #region Save

        /// 현재 씬에 살아있는 파티 상태를 PartySaveData로 빌드
        /// (캐릭터 id + 현재 HP)
        public PartySaveData BuildCurrentPartySaveData()
        {
            var data = new PartySaveData();

            foreach (var player in players)
            {
                if (player == null || player.CharacterData == null) continue;

                var health = player.Health; // Agent 쪽에서 가져오는 프로퍼티라고 가정
                float hp = health != null ? health.CurrentHealth : player.CharacterData.MaxHp;

                data.players.Add(new PlayerSaveData
                {
                    characterId = player.CharacterData.CharacterId,
                    currentHp = hp
                });
            }

            return data;
        }
        
        /// <summary>
        /// 저장된 파티 데이터를 이용해 파티를 스폰 + HP 복원
        /// </summary>
        public void ApplyPartyFromSave(PartySaveData saveData)
        {
            if (saveData == null || saveData.players == null || saveData.players.Count == 0)
            {
                Debug.LogWarning("[PartyManager] 저장된 파티 데이터가 없습니다.");
                return;
            }

            if (playerDatabase == null)
            {
                Debug.LogError("[PartyManager] PlayerDatabaseSo 가 설정되어 있지 않습니다.");
                return;
            }

            // 1) 저장된 순서대로 PlayerDefaultData 리스트 구성
            var partyData = new List<PlayerDefaultData>();
            foreach (var ps in saveData.players)
            {
                var so = playerDatabase.GetById(ps.characterId);
                if (so == null)
                {
                    Debug.LogWarning($"[PartyManager] PlayerDefaultData 를 찾을 수 없습니다. CharacterId={ps.characterId}");
                    continue;
                }
                partyData.Add(so);
            }

            //기본 Spawn 로직으로 캐릭터 생성
            SpawnPartyFromData(partyData);

            //스폰된 캐릭터들에게 저장된 HP 덮어쓰기
            foreach (var ps in saveData.players)
            {
                PlayerDefaultData so = playerDatabase.GetById(ps.characterId);
                if (so == null) continue;

                //같은 CharacterId를 가진 플레이어 찾기
                var player = players.Find(p => p != null &&
                                               p.CharacterData != null &&
                                               p.CharacterData.CharacterId == ps.characterId);

                if (player == null) continue;

                var health = player.Health;
                if (health != null)
                {
                    health.LoadCurrentHealth(ps.currentHp);
                }
            }
        }

        //현재 파티 상태를 SaveManager에 저장
        public void SaveCurrentParty()
        {
            var saveMgr = SaveManager.Instance;
            if (saveMgr == null) return;

            var data = BuildCurrentPartySaveData();
            saveMgr.SaveParty(data);
        }

        #endregion

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