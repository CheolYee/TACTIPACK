using _00.Work.Scripts.Managers;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Events;
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
                    StartEnemyStage(map);
                    break;
                
                case MapType.Shop:
                    OpenShop(map);
                    break;
                
                case MapType.Reward:
                    OpenReward(map);
                    break;
                
                case MapType.Rest:
                    OpenRest(map);
                    break;
                
                case MapType.Random:
                    HandleRandom(map);
                    break;
            }
        }

        private void HandleRandom(MapSo map)
        {
        }

        private void OpenRest(MapSo map)
        {
            if (restRoot != null)
                restRoot.SetActive(true);
        }

        private void OpenReward(MapSo map)
        {
            if (rewardRoot != null)
                rewardRoot.SetActive(true);
        }

        private void OpenShop(MapSo map)
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

        private void StartEnemyStage(MapSo map)
        {
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