using _00.Work.WorkSpace.CheolYee._04.Scripts.Agents;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.AnimatorSystem;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.FSMSystem
{
    public class AgentState
    {
        protected Agent Agent; //가진 객체
        protected AnimParamSo StateParam; //현재 애니메이션 파라미터
        protected bool IsTriggerCall; //트리거가 작동되었는가
        
        protected AgentRenderer Renderer; //렌더러

        //생성자에서 초기화
        public AgentState(Agent agent, AnimParamSo stateParam)
        {
            Agent = agent;
            StateParam = stateParam;
            Renderer = agent.GetCompo<AgentRenderer>();
            
        }

        public virtual void Enter() //진입
        {
            Renderer.SetParam(StateParam, true);
            IsTriggerCall = false;
        }

        public virtual void Update() {} //업데이터

        public virtual void Exit() //변경
        {
            Renderer.SetParam(StateParam, false);
        }
        
        //애니메이션이 끝났다면 트루
        public virtual void AnimationEndTrigger() => IsTriggerCall = true;

    }
}