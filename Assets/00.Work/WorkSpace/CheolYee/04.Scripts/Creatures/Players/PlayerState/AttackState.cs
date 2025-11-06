using System;
using System.Collections;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Agents;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.AnimatorSystem;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Attacks;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.ItemTypes.ActiveItems;
using _00.Work.WorkSpace.CheolYee._04.Scripts.FSMSystem;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Creatures.Players.PlayerState
{
    public class AttackState : PlayerState
    {
        AgentRenderer _agentRenderer;
        AttackExecutor _attackExecutor;
        Coroutine _runCo;
        
        public AttackState(Agent agent, AnimParamSo stateParam) : base(agent, stateParam)
        {
        }

        public override void Enter()
        {
            base.Enter();

            try
            {
                _agentRenderer = Player.GetCompo<AgentRenderer>();
                _attackExecutor = Player.GetCompo<AttackExecutor>();
            }
            catch (NullReferenceException e)
            {
                Debug.LogException(e);
                Debug.LogError($"{Player.name}의 AttackState가 설정될 수 없습니다.");
            }

            _agentRenderer.OnAnimationEnd += OnAnimEnd;
        }

        public override void Exit()
        {
            _agentRenderer.OnAnimationEnd -= OnAnimEnd;

            if (_runCo != null)
            {
                Player.StopCoroutine(_runCo);
                _runCo = null;
            }
            base.Exit();
        }
        private void OnAnimEnd()
        {
            _agentRenderer.OnAnimationEnd -= OnAnimEnd;
            _runCo = Player.StartCoroutine(RunAttackCoroutine());
        }

        private IEnumerator RunAttackCoroutine()
        {
            AttackItemSo item = Agent.ActionData.CurrentAttackItem;
            GameObject target = Agent.ActionData.Target;
            
            yield return _attackExecutor.Perform(item, target);
            
            Player.ChangeState(PlayerStates.IDLE);
        }

    }
}