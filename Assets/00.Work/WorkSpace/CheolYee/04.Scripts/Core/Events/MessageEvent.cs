namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Events
{
    public struct MessageEvent : IEvent
    {
        public readonly string Message;

        public MessageEvent(string message)
        {
            Message = message;
        }
    }
}