using _00.Work.WorkSpace.CheolYee._04.Scripts.Agents;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Events;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.ItemTypes.ActiveItems;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Effects
{
    //플레이어의 스킬이 끝났을 때 발생하는 이벤트
    public struct SkillFinishedEvent : IEvent
    {
        public Agent User {get;}
        public AttackItemSo Item {get;}

        public SkillFinishedEvent(Agent agent, AttackItemSo item)
        {
            User = agent;
            Item = item;
        }
    }
}