using _00.Work.WorkSpace.CheolYee._04.Scripts.Agents;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.AnimatorSystem;
using _00.Work.WorkSpace.CheolYee._04.Scripts.FSMSystem;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Creatures.Players.PlayerState
{
    public class HitState : PlayerState
    {
        public HitState(Agent agent, AnimParamSo stateParam) : base(agent, stateParam)
        {
            Renderer.OnAnimationFire += AnimationEndTrigger;
        }

        public override void Update()
        {
            base.Update();
            if (IsTriggerCall)
                Player.ChangeState(PlayerStates.IDLE);
        }

        public override void Exit()
        {
            Renderer.OnAnimationFire -= AnimationEndTrigger;
            base.Exit();
        }
    }
}