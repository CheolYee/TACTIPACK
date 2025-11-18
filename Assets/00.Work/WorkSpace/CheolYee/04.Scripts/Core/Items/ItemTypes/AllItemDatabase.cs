using System.Collections.Generic;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.ItemTypes
{
    [CreateAssetMenu(fileName = "AllItemDatabase", menuName = "SO/Item/AllItemDatabase", order = 0)]
    public class AllItemDatabase : ScriptableObject
    {
        [field: SerializeField] public List<ItemDataSo> AllItems {get; private set;}
    }
}