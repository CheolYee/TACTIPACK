using _00.Work.WorkSpace.CheolYee._04.Scripts.Agents;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Attacks
{
    public interface IDamageCaster
    {
        void Initialize(Agent agent);

        void OnDamageCast();
    }
}