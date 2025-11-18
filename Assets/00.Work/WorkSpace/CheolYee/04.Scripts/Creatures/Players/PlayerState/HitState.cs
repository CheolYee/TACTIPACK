using _00.Work.WorkSpace.CheolYee._04.Scripts.Agents;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.AnimatorSystem;
using _00.Work.WorkSpace.CheolYee._04.Scripts.FSMSystem;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Creatures.Players.PlayerState
{
    public class HitState : PlayerState //안씀
    {

        private bool _isFirstEnter;
        public HitState(Agent agent, AnimParamSo stateParam) : base(agent, stateParam)
        {
        }

        public override void Enter()
        {
            
            if (!_isFirstEnter)
            {
                Renderer.OnAnimationFire += AnimationEndTrigger;
                _isFirstEnter = true;
            }
            
            IsTriggerCall = false;
            Renderer.SetParam(StateParam, false);
            Renderer.SetParam(StateParam, true);
            
            
            Debug.Log($"HIT Enter");
        }

        public override void Update()
        {
            base.Update();
            if (IsTriggerCall)
                Player.ChangeState(PlayerStates.IDLE);
        }

        public override void Exit()
        {
            base.Exit();
            
            if (_isFirstEnter)
            {
                Renderer.OnAnimationFire -= AnimationEndTrigger;
                _isFirstEnter = false;
            }
            
            Debug.Log("HIT Exit");
        }
    }
}