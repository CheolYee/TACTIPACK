using System;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace _00.Work.WorkSpace.Soso7194._04.Scripts.UI
{
    public class RestUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private CanvasGroup resultGroup;
        [SerializeField] private Button button;
        
        private TextMeshProUGUI _text;
        public static Action OnStartFill;

        private void OnEnable()
        {
            Bar.OnRestEnd += EndRest;
            
            resultGroup.gameObject.SetActive(false);
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(Rest);
            _text.text = "Rest";
        }

        private void OnDisable()
        {
            Bar.OnRestEnd -= EndRest;
        }

        private void Awake()
        {
            _text = GetComponentInChildren<TextMeshProUGUI>();
        }

        private void StartRest()
        {
            _text.text = "Resting...";
            button.onClick.RemoveAllListeners();
            OnStartFill?.Invoke();
        }
        
        private void EndRest()
        {
            _text.text = "Result";
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(Result);
        }

        private void Result()
        {
            resultGroup.gameObject.SetActive(true);
            resultGroup.alpha = 0f;
            resultGroup.DOFade(1f, 0.5f).onComplete += () =>
            {
                _text.text = "Quit";
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(Quit);
            };
        }

        private void Quit()
        {
            canvasGroup.DOFade(0f, 0.5f).onComplete += () =>
            {
                canvasGroup.gameObject.SetActive(false);
            };
        }

        private void Rest()
        {
            StartRest();
        }
    }
}