using System;
using UnityEngine;

public class EnemySaveData : MonoBehaviour
{
    [Serializable]
    public class SaveData
    {
        public GameObject[] enemies;
    }
}
