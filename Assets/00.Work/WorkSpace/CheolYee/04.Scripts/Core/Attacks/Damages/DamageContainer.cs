using System;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Attacks.Damages
{
    [Serializable]
    public struct DamageContainer
    {
        public float Damage;
        public bool IsCritical;

        public DamageContainer(float damage, bool isCritical = false)
        {
            Damage = damage;
            IsCritical = isCritical;
        }
    }
}