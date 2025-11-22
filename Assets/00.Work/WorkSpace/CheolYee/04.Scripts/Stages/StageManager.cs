using _00.Work.Scripts.Managers;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Events;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Managers;
using _00.Work.WorkSpace.CheolYee._04.Scripts.UI.Turn;
using _00.Work.WorkSpace.JaeHun._01._Scrpits;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Stages
{
    public class StageManager : MonoSingleton<StageManager>
    {
        [Header("Stage Roots")]
        [SerializeField] private GameObject shopRoot;
        [SerializeField] private GameObject restRoot;
        [SerializeField] private GameObject rewardRoot;
        [SerializeField] private GameObject randomRoot;
        
        private void OnEnable()
        {
            Bus<MapEnteredEvent>.OnEvent += OnMapEntered;
        }

        private void OnDisable()
        {
            Bus<MapEnteredEvent>.OnEvent -= OnMapEntered;
        }

        private void OnMapEntered(MapEnteredEvent evt)
        {
            Debug.Log($"OnMapEntered: {evt.Map}");
            var map = evt.Map;
            if (map == null) return;

            HideAllStageRoots();
            
            switch (map.mapType)
            {
                case MapType.Enemy:
                    TurnUiContainerPanel.Instance.IsTurnRunning = false;
                    StartEnemyStage(map);
                    break;
                
                case MapType.Shop:
                    TurnUiContainerPanel.Instance.IsTurnRunning = true;
                    OpenShop(map);
                    break;
                
                case MapType.Reward:
                    TurnUiContainerPanel.Instance.IsTurnRunning = true;
                    OpenReward(map);
                    break;
                
                case MapType.Rest:
                    TurnUiContainerPanel.Instance.IsTurnRunning = true;
                    OpenRest(map);
                    break;
                
                case MapType.Random:
                    TurnUiContainerPanel.Instance.IsTurnRunning = true;
                    HandleRandom(map);
                    break;
                
                case MapType.Boss:
                    TurnUiContainerPanel.Instance.IsTurnRunning = false;
                    StartEnemyStage(map);
                    break;
            }
        }

        private void HandleRandom(MapSo map)
        {
            if (randomRoot != null)
                randomRoot.SetActive(true);
        }

        public void OpenRest(MapSo map)
        {
            if (restRoot != null)
                restRoot.SetActive(true);
        }

        public void OpenReward(MapSo map)
        {
            if (rewardRoot != null)
                rewardRoot.SetActive(true);
        }

        public void OpenShop(MapSo map)
        {
            if (shopRoot != null)
                shopRoot.SetActive(true);
        }
        private void HideAllStageRoots()
        {
            if (shopRoot != null) shopRoot.SetActive(false);
            if (restRoot != null) restRoot.SetActive(false);
            if (rewardRoot != null) rewardRoot.SetActive(false);
            if (randomRoot != null) randomRoot.SetActive(false);
        }

        public void StartEnemyStage(MapSo map)
        {
            if (BattleSkillManager.Instance != null)
            {
                BattleSkillManager.Instance.ResetBattleState();
            }
            
            if (EnemySpawnManager.Instance != null)
            {
                EnemySpawnManager.Instance.SetSpawnEnemy(map);
            }
            else
            {
                Debug.LogError("[StageManager] EnemySpawnManager 인스턴스를 찾을 수 없습니다.");
            }
        }
    }
}