using System;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.ItemTypes;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _00.Work.WorkSpace.Soso7194._04.Scripts.UI
{
    public class Reward : MonoBehaviour
    {
        [SerializeField] private CanvasGroup itemCanvas;
        
        private Image _image;
        private TextMeshProUGUI _text;

        private void Awake()
        {
            _image = itemCanvas.GetComponentInChildren<Image>();
            _text = itemCanvas.GetComponentInChildren<TextMeshProUGUI>();
        }

        public void OnAnimationEnd()
        {
            Debug.Log("Animation End");
            Fade();
        }

        private void Fade()
        {
            itemCanvas.gameObject.SetActive(true);
            itemCanvas.alpha = 0f;
            itemCanvas.DOFade(1f, 1.5f);

        }
    }
}