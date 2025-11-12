using System;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.ItemTypes.ActiveItems;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Attacks.Skills
{
    public interface ISkillHandler
    {
        event Action OnComplete;
        void Init(SkillContent ctx, AttackItemSo item);
        void NotifyImpact();
        void NotifyEnd();
    }
}