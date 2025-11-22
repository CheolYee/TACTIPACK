using System;
using System.Collections.Generic;
using _00.Work.Resource.Scripts.SO;
using UnityEngine;

namespace _00.Work.Scripts.SO
{
    [Serializable]

    [CreateAssetMenu(fileName = "PoolingList", menuName = "SO/Pool/List", order = 0)]
    public class PoolingListSo : ScriptableObject
    {
        public List<PoolItem> items;
    }
}