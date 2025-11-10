using _00.Work.WorkSpace.CheolYee._04.Scripts.Agents;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.AnimatorSystem;
using _00.Work.WorkSpace.CheolYee._04.Scripts.FSMSystem;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Creatures.Players.PlayerState
{
    public class PlayerState : AgentState
    {
        protected Player Player;
        
        public PlayerState(Agent agent, AnimParamSo stateParam) : base(agent, stateParam)
        {
            Player = agent as Player;
        }
    }
}