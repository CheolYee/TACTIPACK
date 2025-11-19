using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Attacks.Damages;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Attacks
{
    public interface IDamageable
    {
        void ApplyDamage(DamageContainer attackData);
    }
}