using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.AnimatorSystem;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.FSMSystem
{
    [CreateAssetMenu(fileName = "New State", menuName = "SO/FSM/State", order = 0)]
    public class StateSo : ScriptableObject
    {
        public string stateName; //스테이트 이름
        public string className; //클래스 이름
        public int stateIndex; //인덱스
        public AnimParamSo paramSo; //파리미터
    }
}