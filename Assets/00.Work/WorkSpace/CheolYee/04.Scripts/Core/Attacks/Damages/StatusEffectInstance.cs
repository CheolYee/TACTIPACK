using System;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Attacks.Damages
{
    [Serializable]
    public class StatusEffectInstance
    {
        public StatusEffectType Type;
        public int RemainingTurns;
        public int StackCount;
        public float Power;

        public StatusEffectInstance(StatusEffectType type, int duration, float power)
        {
            Type = type;
            RemainingTurns = duration;
            Power = power;
            StackCount = 1;
        }
    }
}