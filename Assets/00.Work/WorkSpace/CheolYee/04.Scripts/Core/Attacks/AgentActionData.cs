using System;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.ItemTypes.ActiveItems;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Attacks
{
    [Serializable]
    public struct AgentActionData
    {
        [field: SerializeField] public AttackDataSo LastAttackData { get; set; }
        [field: SerializeField] public GameObject Target { get; set; }
        [field: SerializeField] public AttackItemSo CurrentAttackItem { get; set; }
    }
}