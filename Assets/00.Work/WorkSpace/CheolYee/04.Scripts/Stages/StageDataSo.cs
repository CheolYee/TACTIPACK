using System;
using System.Collections.Generic;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Creatures.Enemies;
using UnityEngine;
using UnityEngine.Serialization;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Stages
{
    [Serializable]
    public class StageEnemyEntry
    {
        public int spawnIndex;
        public EnemyDefaultData enemyData;
    }
    
    [CreateAssetMenu(fileName = "new Stage Data", menuName = "SO/Stage/Data", order = 0)]
    public class StageDataSo : ScriptableObject
    {
        public string stageName;
        
        [Header("Enemies")]
        public List<StageEnemyEntry> enemies = new();
    }
}