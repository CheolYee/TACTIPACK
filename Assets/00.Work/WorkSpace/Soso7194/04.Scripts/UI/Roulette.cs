using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

// 추가
using _00.Work.Resource.Scripts.Managers;                // FadeManager
using _00.Work.WorkSpace.CheolYee._04.Scripts.Stages;
using _00.Work.WorkSpace.CheolYee._04.Scripts.UI.Turn; // StageManager
using _00.Work.WorkSpace.JaeHun._01._Scrpits;           // MapManager, MapSo

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

        private void Start()
        {
            DecideResultAtGameStart();
        }

        private void OnEnable()
        {
            if (wheel != null)
            {
                // 기존 new Quaternion(...) 대신 안전하게 Euler 사용
                wheel.rotation = Quaternion.Euler(0f, 0f, 45f);
            }

            if (spinButton != null)
            {
                spinButton.onClick.RemoveAllListeners();
                spinButton.onClick.AddListener(Spin);
                spinButton.interactable = true;
            }

            if (text != null)
                text.text = "돌리기";

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
                canvasGroup.gameObject.SetActive(true);
            }
        }

        private void DecideResultAtGameStart()
        {
            int value = Random.Range(0, 100);

            if (value < 85) _fixedResult = ResultType.Enemy;
            else if (value < 90) _fixedResult = ResultType.Shop;
            else if (value < 95) _fixedResult = ResultType.Chest;
            else _fixedResult = ResultType.Rest;
        }

        private void Spin()
        {
            if (wheel == null || spinButton == null || text == null)
                return;
            
            SoundManager.Instance.PlaySfx(SfxId.Dollimpan);

            text.text = "회전 중";
            spinButton.onClick.RemoveAllListeners();
            spinButton.interactable = false;

            float targetAngle = GetTargetAngle(_fixedResult);
            float finalAngle = -(targetAngle + 360f * extraSpins);

            wheel
                .DORotate(new Vector3(0, 0, finalAngle), spinDuration, RotateMode.FastBeyond360)
                .SetEase(Ease.OutQuart)
                .OnComplete(() =>
                {
                    text.text = "확인";     // 결과 확정 후 확인 버튼
                    spinButton.onClick.RemoveAllListeners();
                    spinButton.onClick.AddListener(Exit);
                    spinButton.interactable = true;
                });
        }

        private float GetTargetAngle(ResultType type)
        {
            switch (type)
            {
                case ResultType.Enemy:
                    // 적 영역 대충 가운데
                    return 306f * Random.Range(0.2f, 0.7f);
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
            if (canvasGroup == null)
                return;

            // 중복 입력 방지
            if (!canvasGroup.interactable)
                return;

            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            if (spinButton != null) spinButton.interactable = false;

            // 전역 페이드 인/아웃 (화면 자연스럽게 전환용)
            FadeManager.Instance?.FadeIn(() =>
            {
                canvasGroup.gameObject.SetActive(false);

                var mapMgr   = MapManager.Instance;
                var stageMgr = StageManager.Instance;

                if (mapMgr == null || stageMgr == null)
                    return;

                MapSo currentMap = mapMgr.CurrentMapInStage;
                if (currentMap == null)
                    return;

                switch (_fixedResult)
                {
                    case ResultType.Enemy:
                        // 일반/보스 방과 동일하게 적 스폰
                        TurnUiContainerPanel.Instance.IsTurnRunning = false;
                        stageMgr.StartEnemyStage(currentMap);
                        break;

                    case ResultType.Shop:
                        TurnUiContainerPanel.Instance.IsTurnRunning = true;
                        stageMgr.OpenShop(currentMap);
                        break;

                    case ResultType.Chest:
                        // 보물 = Reward 방
                        TurnUiContainerPanel.Instance.IsTurnRunning = true;
                        stageMgr.OpenReward(currentMap);
                        break;

                    case ResultType.Rest:
                        TurnUiContainerPanel.Instance.IsTurnRunning = true;
                        stageMgr.OpenRest(currentMap);
                        break;
                }
                
                FadeManager.Instance?.FadeOut();
                
            });

        }
    }
}
