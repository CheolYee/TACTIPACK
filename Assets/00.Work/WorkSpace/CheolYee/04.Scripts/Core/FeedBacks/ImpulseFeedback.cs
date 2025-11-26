using Unity.Cinemachine;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.FeedBacks
{
    public class ImpulseFeedback : FeedBack
    {
        [SerializeField] private CinemachineImpulseSource cinemachineImpulseSource;
        [SerializeField] private float impulseForce = 0.1f;
        public override void CreateFeedback()
        {
            cinemachineImpulseSource.GenerateImpulse(impulseForce);
        }

        public override void FinishFeedback()
        {
        }
    }
}