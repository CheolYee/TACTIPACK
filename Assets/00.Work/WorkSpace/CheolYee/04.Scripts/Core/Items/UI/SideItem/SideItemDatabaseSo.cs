using System.Collections.Generic;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.UI.SideItem
{
    [CreateAssetMenu(fileName = "SideItemDatabase", menuName = "SO/Item/Side Item Database", order = 0)]
    public class SideItemDatabaseSo : ScriptableObject
    {
        [Header("Side Item Database")]
        [field: SerializeField] public List<ItemDataSo> SideItemDatabase { get; private set; }
    }
}