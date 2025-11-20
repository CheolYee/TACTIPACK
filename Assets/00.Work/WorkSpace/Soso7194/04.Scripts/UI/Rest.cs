using System;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace _00.Work.WorkSpace.Soso7194._04.Scripts.UI
{
    public class Rest : MonoBehaviour
    {
        [field:SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Button button;
        
        private TextMeshProUGUI _text;
        public static Action OnStartFill;

        private void OnEnable()
        {
            Bar.OnRestEnd += EndRest;
        }

        private void OnDisable()
        {
            Bar.OnRestEnd -= EndRest;
        }

        private void Awake()
        {
            _text = GetComponentInChildren<TextMeshProUGUI>();
        }

        public void StartRest()
        {
            _text.text = "Resting...";
            button.onClick.RemoveAllListeners();
            OnStartFill?.Invoke();
        }
        
        private void EndRest()
        {
            _text.text = "Quit";
            button.onClick.AddListener(Quit);
        }

        private void Quit()
        {
            canvasGroup.DOFade(0f, 0.5f).onComplete += () =>
            {
                canvasGroup.gameObject.SetActive(false);
            };
        }
    }
}