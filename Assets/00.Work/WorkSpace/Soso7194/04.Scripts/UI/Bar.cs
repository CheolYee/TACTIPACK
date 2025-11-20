using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace _00.Work.WorkSpace.Soso7194._04.Scripts.UI
{
    public class Bar : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        [SerializeField] private float minFillTime = 1f; // 최소 시간
        [SerializeField] private float maxFillTime = 3f; // 최대 시간

        public static Action OnRestEnd;

        private void OnEnable()
        {
            Rest.OnStartFill += StartFill;
            slider.value = 0f;
        }

        private void OnDisable()
        {
            Rest.OnStartFill -= StartFill;
            slider.value = 0f;
        }

        public void StartFill()
        {
            float randomTime = Random.Range(minFillTime, maxFillTime);
            StartCoroutine(FillSlider(randomTime));
        }

        private IEnumerator FillSlider(float fillTime)
        {
            float elapsed = 0f;

            while (elapsed < fillTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fillTime);
                slider.value = t;
                yield return null;
            }

            slider.value = 1f;
            OnRestEnd?.Invoke();
        }
    }
}