using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Events
{
    //쿨타임 시작
    public struct ItemCooldownStartedEvent : IEvent
    {
        public ItemInstance Instance { get; }
        public int CooldownTurns { get; }

        public ItemCooldownStartedEvent(ItemInstance instance, int cooldownTurns)
        {
            Instance = instance;
            CooldownTurns = cooldownTurns;
        }
    }

    //쿨타임 턴수 변경
    public struct ItemCooldownChangedEvent : IEvent
    {
        public ItemInstance Instance { get; }
        public int RemainingTurns { get; }

        public ItemCooldownChangedEvent(ItemInstance instance, int remainingTurns)
        {
            Instance = instance;
            RemainingTurns = remainingTurns;
        }
    }
}