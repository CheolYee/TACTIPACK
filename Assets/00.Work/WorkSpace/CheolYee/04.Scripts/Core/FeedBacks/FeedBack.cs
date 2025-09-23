using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.FeedBacks
{
    public abstract class FeedBack : MonoBehaviour
    {
        public abstract void CreateFeedback();
    
        public abstract void FinishFeedback();

        public void OnDisable()
        {
            FinishFeedback();
        }
    }
}