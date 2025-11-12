using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Attacks.Skills;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Attacks
{
    public class AnimationEventRelay : MonoBehaviour
    {
        public ISkillHandler Handler;
        
        public void SkillImpact() => Handler?.NotifyImpact();
        public void SkillEnd() => Handler?.NotifyEnd();
    }
}