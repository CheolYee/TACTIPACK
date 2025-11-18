namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Events
{
    public enum BattleResultType
    {
        Victory, //승리
        Defeat //패배
    }

    public struct BattleResultEvent : IEvent
    {
        public BattleResultType Result;

        public BattleResultEvent(BattleResultType result)
        {
            Result = result;
        }
    }
}