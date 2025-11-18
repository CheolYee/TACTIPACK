using _00.Work.WorkSpace.CheolYee._04.Scripts.Agents;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.AnimatorSystem;
using _00.Work.WorkSpace.CheolYee._04.Scripts.FSMSystem;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Creatures.Players.PlayerState
{
    public class PlayerDeathState : PlayerState
    {
        public PlayerDeathState(Agent agent, AnimParamSo stateParam) : base(agent, stateParam)
        {
        }

        public override void Update()
        {
            base.Update();
            if (!Agent.IsDead) 
                Player.ChangeState(PlayerStates.IDLE);
        }
    }
}