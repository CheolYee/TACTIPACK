using _00.Work.Resource.Scripts.Managers;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Events;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Stages;
using _00.Work.WorkSpace.JaeHun._01._Scrpits;
using UnityEngine;
using UnityEngine.UI;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.UI.BattleResult
{
    public class BattleResultUi : MonoBehaviour
    {
        [Header("Result Panels")]
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private GameObject defeatPanel;
        
        [Header("Result Buttons")]
        [SerializeField] private Button victoryButton;
        [SerializeField] private Button defeatButton;
        
        [Header("Game Over UI")]
        [SerializeField] private GameObject gameOverRoot;

        private void OnEnable()
        {
            Bus<BattleResultEvent>.OnEvent += OnBattleResult;
        }

        private void OnDisable()
        {
            Bus<BattleResultEvent>.OnEvent -= OnBattleResult;
        }

        private void Start()
        {
            if (victoryPanel != null) victoryPanel.SetActive(false);
            if (defeatPanel != null) defeatPanel.SetActive(false);
            if (victoryButton != null) victoryButton.onClick.AddListener(ShowMapUI);
            if (defeatButton != null) defeatButton.onClick.AddListener(ShowGameOverUI);
        }

        private void OnBattleResult(BattleResultEvent evt)
        {
            switch (evt.Result)
            {
                case BattleResultType.Defeat:
                    defeatPanel.SetActive(true);
                    break;
                case BattleResultType.Victory:
                    victoryPanel.SetActive(true);
                    break;
            }
        }
        
        private void ShowGameOverUI()
        {
            if (victoryPanel != null) victoryPanel.SetActive(false);
            if (defeatPanel != null) defeatPanel.SetActive(false);

            var fade = FadeManager.Instance;

            // 페이드로 자연스럽게 전환
            if (fade != null)
            {
                fade.FadeIn(() =>
                {
                    if (gameOverRoot != null)
                        gameOverRoot.SetActive(true);

                    // 배경이 까맣게 덮여 있는 상태에서
                    // 게임 오버 UI만 보이게 할 거면 FadeOut은 안 해도 됨.
                    // 필요하면 아래 주석 풀어서 다시 페이드 아웃
                    // fade.FadeOut();
                });
            }
            else
            {
                // 페이드가 없다면 그냥 즉시 오픈
                if (gameOverRoot != null)
                    gameOverRoot.SetActive(true);
            }
        }
        
        private void ShowMapUI()
        {
            if (victoryPanel != null) victoryPanel.SetActive(false);
            if (defeatPanel != null) defeatPanel.SetActive(false);

            var fade = FadeManager.Instance;
            var mapManager = MapManager.Instance;

            // 방어 코드: 매니저 몇 개 없어도 터지진 않게
            if (fade == null || mapManager == null)
            {
                Debug.LogWarning("[BattleResultUi] Fade/Map/Stage 매니저 중 일부가 없습니다. 즉시 처리합니다.");

                // 맵 해금
                mapManager?.CompleteCurrentMap();
                // 전투 Root 끄기
                // 맵 UI 다시 표시 (애니메이션 없이)
                mapManager?.ShowCurrentChapterRoot(false);
                return;
            }

            // 페이드 인 → 전환 작업 → 페이드 아웃
            fade.FadeIn(() =>
            {
                // 1) 전투 UI Root 끄기
                // 2) 지금 맵 클리어 처리 (다음 노드 해금)
                mapManager.CompleteCurrentMap();

                // 3) 현재 챕터 맵 UI 다시 띄우기 (밑에서 위로 슬라이드)
                mapManager.ShowCurrentChapterRoot(true);

                // 4) 다시 페이드 아웃으로 전환
                fade.FadeOut();
            });
        }
    }
}