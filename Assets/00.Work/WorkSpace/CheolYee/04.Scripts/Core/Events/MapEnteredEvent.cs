using _00.Work.WorkSpace.JaeHun._01._Scrpits;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Events
{
    public struct MapEnteredEvent : IEvent
    {
        public MapSo Map;
        
        public MapEnteredEvent(MapSo map) => Map = map;
    }
}