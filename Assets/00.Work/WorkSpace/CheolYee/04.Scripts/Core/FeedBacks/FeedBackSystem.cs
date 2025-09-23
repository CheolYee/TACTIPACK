using System.Collections.Generic;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.FeedBacks
{
    public class FeedBackSystem : MonoBehaviour
    {
        [SerializeField] private List<FeedBack> feedBacks;

        public void PlayFeedback()
        {
            feedBacks.ForEach(feedBack => feedBack.FinishFeedback());
            feedBacks.ForEach(feedBack => feedBack.CreateFeedback());
        }
    }
}