using System;
using _00.Work.Resource.Scripts.Managers;
using _00.Work.WorkSpace.JaeHun._01._Scrpits;
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
            _text.text = "휴식하기";
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
            _text.text = "휴식중..";
            button.onClick.RemoveAllListeners();
            OnStartFill?.Invoke();
        }
        
        private void EndRest()
        {
            _text.text = "결과 보기";
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(Result);
        }

        private void Result()
        {
            resultGroup.gameObject.SetActive(true);
            resultGroup.alpha = 0f;
            resultGroup.DOFade(1f, 0.5f).onComplete += () =>
            {
                _text.text = "돌아가기";
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(Quit);
            };
        }

        private void Quit()
        {
            if (canvasGroup == null)
                return;

            // 연속 클릭 방지
            button.interactable = false;

            FadeManager.Instance.FadeIn(() =>
            {
                canvasGroup.gameObject.SetActive(false);
                button.interactable = true;

                var mapMgr = MapManager.Instance;
                if (mapMgr != null)
                {
                    mapMgr.CompleteCurrentMap();
                    mapMgr.ShowCurrentChapterRoot(true);
                }
                
                FadeManager.Instance.FadeOut();
            });
        }


        private void Rest()
        {
            StartRest();
        }
    }
}