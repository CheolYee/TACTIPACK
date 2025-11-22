using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.ItemTypes.PassiveItems;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Events
{
    public struct PassiveItemEquippedEvent : IEvent
    {
        public PassiveItemSo Item { get; }
        public ItemInstance Inst { get; }

        public PassiveItemEquippedEvent(PassiveItemSo item, ItemInstance inst)
        {
            Item = item;
            Inst = inst;
        }
    }
    
    public struct PassiveItemUnequippedEvent : IEvent
    {
        public PassiveItemSo Item { get; }
        public ItemInstance Inst { get; }

        public PassiveItemUnequippedEvent(PassiveItemSo item, ItemInstance inst)
        {
            Item = item;
            Inst = inst;
        }
    }
}