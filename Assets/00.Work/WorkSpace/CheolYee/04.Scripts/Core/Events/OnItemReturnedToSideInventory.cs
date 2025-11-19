using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Events
{
    public struct OnItemReturnedToSideInventory : IEvent
    {
        public ItemInstance Inst;

        public OnItemReturnedToSideInventory(ItemInstance item)
        {
            Inst = item;
        }
    }
}