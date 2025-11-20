using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace _00.Work.WorkSpace.Soso7194._04.Scripts.UI
{
    public class Roulette : MonoBehaviour
    {
        [Header("Roulette")]
        [SerializeField] private Transform wheel;

        [Header("Spin Settings")]
        [SerializeField] private float spinDuration = 4f; 
        [SerializeField] private int extraSpins = 5;

        [Header("UI")]
        [SerializeField] private Button spinButton;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProUGUI text;

        private enum ResultType
        {
            Enemy,
            Shop,
            Chest,
            Rest
        }

        private ResultType _fixedResult;
        
        void Start()
        {
            DecideResultAtGameStart();
        }

        private void OnEnable()
        {
            wheel.gameObject.transform.rotation = new Quaternion(0f, 0f, 45f, 0f);
            spinButton.onClick.RemoveAllListeners();
            spinButton.onClick.AddListener(Spin);
            text.text = "Spin";
        }

        void DecideResultAtGameStart()
        {
            int value = Random.Range(0, 100);

            if (value < 85) _fixedResult = ResultType.Enemy;
            else if (value < 90) _fixedResult = ResultType.Shop;
            else if (value < 95) _fixedResult = ResultType.Chest;
            else _fixedResult = ResultType.Rest;

            Debug.Log($"[Roulette] 이번 스테이지 결과 = {_fixedResult}");
        }

        private void Spin()
        {
            text.text = "Spinning...";
            spinButton.onClick.RemoveAllListeners();
            
            float targetAngle = GetTargetAngle(_fixedResult);
            float finalAngle = -(targetAngle + 360f * extraSpins);

            wheel.DORotate(new Vector3(0, 0, finalAngle), spinDuration, RotateMode.FastBeyond360)
                .SetEase(Ease.OutQuart)
                .OnComplete(() =>
                {
                    Debug.Log($"[Roulette] 최종 멈춤: {_fixedResult}");
                    text.text = "Quit";
                    spinButton.onClick.RemoveAllListeners();
                    spinButton.onClick.AddListener(Exit);
                });
        }

        float GetTargetAngle(ResultType type)
        {
            switch (type)
            {
                case ResultType.Enemy:
                    return 306f * Random.Range(0.2f, 0.7f); // 적 영역 중심
                case ResultType.Shop:
                    return 306f + Random.Range(6f, 12f);
                case ResultType.Chest:
                    return 324f + Random.Range(6f, 12f);
                case ResultType.Rest:
                    return 342f + Random.Range(6f, 12f);
            }

            return 0f;
        }

        private void Exit()
        {
            canvasGroup.DOFade(0f, 0.5f).onComplete += () =>
            {
                canvasGroup.gameObject.SetActive(false);
            };
        }
    }
}