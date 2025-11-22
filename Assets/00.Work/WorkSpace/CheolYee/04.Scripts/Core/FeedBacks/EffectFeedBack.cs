using _00.Work.Resource.Scripts.Managers;
using _00.Work.Resource.Scripts.SO;
using _00.Work.Scripts.Managers;
using _00.Work.Scripts.SO;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Effects;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.FeedBacks
{
    public class EffectFeedBack : FeedBack
    {
        [SerializeField] private PoolItem effectItem;
        public override void CreateFeedback()
        {
            EffectSystem effect = PoolManager.Instance.Pop(effectItem.poolName) as EffectSystem;
            if (effect != null) effect.SetPosAndPlay(transform.position);
        }

        public override void FinishFeedback()
        {
        }
    }
}