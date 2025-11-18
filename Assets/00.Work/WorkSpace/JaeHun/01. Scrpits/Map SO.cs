using _00.Work.WorkSpace.CheolYee._04.Scripts.Stages;
using UnityEngine;
using UnityEngine.Serialization;

namespace _00.Work.WorkSpace.JaeHun._01._Scrpits
{
    public enum MapType
    {
        Enemy, Shop, Reward, Rest, Random
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
    }
}
