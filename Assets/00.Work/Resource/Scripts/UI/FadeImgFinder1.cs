using _00.Work.Resource.Scripts.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace _00.Work.Resource.Manager
{
    public class FadeImgFinder : MonoBehaviour
    {
        private void Awake()
        {
            FadeManager.Instance.fadeImage = gameObject.GetComponent<Image>();
            FadeManager.Instance.FadeOut();
        }
    }
}