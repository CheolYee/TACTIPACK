using _00.Work.WorkSpace.CheolYee._04.Scripts.Stages;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Events
{
    public struct StageSelectedEvent : IEvent
    {
        public StageDataSo Stage;
        
        StageSelectedEvent(StageDataSo stage)
        {
            Stage = stage;
        }
    }
}