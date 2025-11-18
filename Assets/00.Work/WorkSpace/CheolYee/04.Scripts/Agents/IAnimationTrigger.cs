using System;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Agents
{
    public interface IAnimationTrigger
    {
        event Action OnAnimationEnd;
        event Action OnAnimationFire;
    }
}