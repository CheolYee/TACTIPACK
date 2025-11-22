using System;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Stages;
using UnityEngine;
using UnityEngine.Serialization;

namespace _00.Work.WorkSpace.JaeHun._01._Scrpits
{
    public enum MapType
    {
        Enemy, Shop, Reward, Rest, Random, Boss
    }
    
    [CreateAssetMenu(fileName = "MapSO", menuName = "Scriptable Objects/MapSO")]
    public class MapSo : ScriptableObject
    {
        [Header("Map Data")]
        public string mapName;
        public MapType mapType;
        public MapSo[] nextMap;
        public bool isLock = true; //처음 시작했을 때 열려있는노드면 true
        
        [Header("Stage Data")]
        public StageDataSo stageData;

        private void OnValidate()
        {
            switch (mapType)
            {
                case MapType.Enemy:
                    mapName = "일반 방";
                    break;
                case MapType.Random:
                    mapName = "랜덤 방";
                    break;
                case MapType.Shop:
                    mapName = "상점 방";
                    break;
                case MapType.Rest:
                    mapName = "휴식 방";
                    break;
                case MapType.Reward:
                    mapName = "보상 방";
                    break;
                case MapType.Boss:
                    mapName = "보스 방";
                    break;
            }
        }
    }
}
