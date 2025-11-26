using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.ItemTypes.ActiveItems;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Attacks
{
    public interface IAttackable
    {
        void Attack(AttackItemSo item, ItemInstance inst = null);
    }
}