using System;
using System.Collections.Generic;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Agents;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.FSMSystem
{
    public class AgentStateMachine
    {
        public AgentState CurrentState { get; private set; }

        private Dictionary<int, AgentState> _stateDict;

        public AgentStateMachine(Agent agent, StateSo[] stateList) //생성자
        {
            _stateDict = new Dictionary<int, AgentState>(); //딕셔너리 초기화

            foreach (StateSo state in stateList)
            {
                Type type = Type.GetType(state.className); //타입 가져오기
                Debug.Assert(type != null, nameof(type) + " is null"); //안전코드
                AgentState newState = Activator.CreateInstance(type, agent, state.paramSo) as AgentState; //새 상태 인스턴스 생성
                
                _stateDict.Add(state.stateIndex, newState); //상태 딕셔너리에 저장
            }
        }

        //현재 FSM의 상태를 변경합니다.
        public void ChangeState(int newStateIndex)
        {
            CurrentState?.Exit(); //현재 상태 퇴장 처리
            AgentState newState = _stateDict.GetValueOrDefault(newStateIndex); //키로 상태 찾아오기
            Debug.Assert(newState != null, nameof(newState) + " is null"); //방지코드
            
            CurrentState = newState;
            CurrentState.Enter();
        }

        //현재 상태의 업데이트를 실행합니다.
        public void UpdateMachine()
        {
            CurrentState?.Update();
        }
    }
}