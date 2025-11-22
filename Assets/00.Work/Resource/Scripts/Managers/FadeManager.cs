using System;
using System.Collections;
using _00.Work.Scripts.Managers;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _00.Work.Resource.Scripts.Managers
{
    public class FadeManager : MonoSingleton<FadeManager>
    {
        [Header("Fade UI")]
        [SerializeField] private CanvasGroup fadeCanvasGroup;
        public float fadeDuration = 1f;
        
        protected override void Awake()
        {
            base.Awake();
            if (Instance == this)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        private void Start()
        {
            FadeOut();
        }

        public void FadeOut(Action onComplete = null)
        {
            if (fadeCanvasGroup == null)
                return;

            fadeCanvasGroup.gameObject.SetActive(true);
            fadeCanvasGroup.alpha = 1f;
            fadeCanvasGroup.interactable = true;
            fadeCanvasGroup.blocksRaycasts = true;

            fadeCanvasGroup.DOFade(0f, fadeDuration).OnComplete(() =>
            {
                fadeCanvasGroup.gameObject.SetActive(false);
                fadeCanvasGroup.interactable = false;
                fadeCanvasGroup.blocksRaycasts = false;
                onComplete?.Invoke();
            });
        }

        public void FadeIn(Action onFadeComplete = null)
        {
            if (fadeCanvasGroup == null)
                return;

            fadeCanvasGroup.gameObject.SetActive(true);
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.interactable = true;
            fadeCanvasGroup.blocksRaycasts = true;

            fadeCanvasGroup.DOFade(1f, fadeDuration).OnComplete(() =>
            {
                onFadeComplete?.Invoke();
            });
        }

        private void FadeToScene(int sceneIndex)
        {
            FadeIn(() =>
            {
                DOTween.KillAll();
                SceneManager.LoadScene(sceneIndex);
                FadeOut();
            });
        }

        public void FadeInOut(Action onFadeComplete = null)
        {
            FadeIn(() =>
            {
                DOTween.KillAll();
                FadeOut(onFadeComplete);
            });
        }


        public void FadeToSceneDelay(int sceneIndex)
        {
            StartCoroutine(DelayAndFadeToScene(sceneIndex));
        }

        private IEnumerator DelayAndFadeToScene(int sceneIndex)
        {
            yield return null; // 한 프레임 대기: 모든 Awake() 보장
            FadeToScene(sceneIndex);
        }
        
        public void FadeToSceneAsync(int sceneIndex, float minLoadingTime = 1)
        {
            StartCoroutine(FadeAndLoadSceneCoroutine(sceneIndex, minLoadingTime));
        }
        
        private IEnumerator FadeAndLoadSceneCoroutine(int sceneIndex, float minLoadingTime)
        {
            bool fadeInDone = false;
            FadeIn(() => fadeInDone = true);

            while (!fadeInDone)
                yield return null;

            AsyncOperation op = SceneManager.LoadSceneAsync(sceneIndex);
            if (op != null)
            {
                op.allowSceneActivation = false;

                float timer = 0f;

                while (op.progress < 0.9f)
                {
                    timer += Time.deltaTime;
                    yield return null;
                }

                while (timer < minLoadingTime)
                {
                    timer += Time.deltaTime;
                    yield return null;
                }

                op.allowSceneActivation = true;
            }

            yield return null;

            FadeOut();
        }
        
    }
}