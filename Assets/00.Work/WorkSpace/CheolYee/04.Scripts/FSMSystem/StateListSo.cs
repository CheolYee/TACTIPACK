using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.FSMSystem
{
    [CreateAssetMenu(fileName = "Fsm State Manager", menuName = "SO/FSM/ListManager", order = 5)]
    public class StateListSo : ScriptableObject
    {
        public string enumName;
        public StateSo[] states;
    }
}