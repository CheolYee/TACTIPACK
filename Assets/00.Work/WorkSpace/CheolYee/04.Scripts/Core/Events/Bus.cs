namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Events
{
    public static class Bus<T> where T : IEvent
    {
        public delegate void Event(T evt);
        
        public static event Event OnEvent;
        public static void Raise(T evt) => OnEvent?.Invoke(evt);
    }

    public interface IEvent //태깅
    {
    }
}