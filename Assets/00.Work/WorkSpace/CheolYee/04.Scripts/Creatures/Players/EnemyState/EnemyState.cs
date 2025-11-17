using _00.Work.WorkSpace.CheolYee._04.Scripts.Agents;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.AnimatorSystem;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Creatures.Enemies;
using _00.Work.WorkSpace.CheolYee._04.Scripts.FSMSystem;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Creatures.Players.EnemyState
{
    public class EnemyState : AgentState
    {
        protected Enemy Enemy;
            
        public EnemyState(Agent agent, AnimParamSo stateParam) : base(agent, stateParam)
        {
            Enemy = agent as Enemy;
        }

    }
}