using _00.Work.WorkSpace.CheolYee._04.Scripts.Agents;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.AnimatorSystem;
using _00.Work.WorkSpace.CheolYee._04.Scripts.FSMSystem;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Creatures.Players.EnemyState
{
    public class EnemyDeathState : EnemyState
    {
        public EnemyDeathState(Agent agent, AnimParamSo stateParam) : base(agent, stateParam)
        {
        }
        
        public override void Update()
        {
            base.Update();
            if (!Agent.IsDead) 
                Enemy.ChangeState(EnemyStates.IDLE);
        }
    }
}