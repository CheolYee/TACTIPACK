using _00.Work.Scripts.Managers;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Events;
using _00.Work.WorkSpace.JaeHun._01._Scrpits;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Stages
{
    public class StageManager : MonoSingleton<StageManager>
    {
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
        }

        private void OpenReward(MapSo map)
        {
        }

        private void OpenShop(MapSo map)
        {
        }

        private void StartEnemyStage(MapSo map)
        {
            EnemySpawnManager.Instance.SetSpawnEnemy(map);
        }
    }
}