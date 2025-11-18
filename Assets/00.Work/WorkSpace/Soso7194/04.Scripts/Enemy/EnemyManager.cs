 using System;
 using UnityEngine;

namespace _00.Work.WorkSpace.Soso7194._04.Scripts.Enemy
{
    public class EnemyManager : MonoBehaviour
    {
        private EnemiesSpawn spawner; 
        private RandomTargeting targeting;
        
        private void Awake()
        {
            spawner = GetComponent<EnemiesSpawn>();
            targeting = GetComponent<RandomTargeting>();
        }
    }
}
