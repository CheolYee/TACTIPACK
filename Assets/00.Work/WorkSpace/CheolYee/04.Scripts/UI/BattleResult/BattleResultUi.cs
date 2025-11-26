using _00.Work.Resource.Scripts.Managers;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Events;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.UI.SideItem;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Managers;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Save;
using _00.Work.WorkSpace.CheolYee._04.Scripts.UI.Turn;
using _00.Work.WorkSpace.JaeHun._01._Scrpits;
using TMPro;
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
        
        [Header("Reward UI")]
        [SerializeField] private TextMeshProUGUI coinRewardText; // 승리 시 획득 골드 표시용
        
        [Header("Scenes")]
        [SerializeField] private int mainMenuSceneIndex;

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
            if (coinRewardText != null) coinRewardText.gameObject.SetActive(false);
        }

        private void OnBattleResult(BattleResultEvent evt)
        {
            var speedMgr = BattleSpeedManager.Instance;
            if (speedMgr != null)
            {
                speedMgr.SetNormalSpeed();
            }
            
            switch (evt.Result)
            {
                case BattleResultType.Defeat:
                    defeatPanel.SetActive(true);
                    break;
                case BattleResultType.Victory:
                    SoundManager.Instance.PlaySfx(SfxId.Victory);
                    int reward = Random.Range(300, 500);

                    var item = SideInventoryManager.Instance.AddRandomItem();
                    var itemName = item.itemName;
                    
                    if (reward > 0)
                    {
                        var money = MoneyManager.Instance;
                        if (money != null)
                        {
                            money.AddCoin(reward);
                        }
                    }
                    
                    if (reward > 0)
                    {
                        coinRewardText.gameObject.SetActive(true);
                        coinRewardText.text = $"+{reward}G\n획득 아이템: {itemName}";
                    }
                    else
                    {
                        coinRewardText.gameObject.SetActive(false);
                    }
                    
                    victoryPanel.SetActive(true);
                    break;
            }
            TurnUiContainerPanel.Instance.IsTurnRunning = true;
        }
        
        private void ShowGameOverUI()
        {
            if (victoryPanel != null) victoryPanel.SetActive(false);
            if (defeatPanel != null) defeatPanel.SetActive(false);
            
            var saveMgr = SaveManager.Instance;
            if (saveMgr != null)
            {
                saveMgr.DeleteAllSaves();
            }

            // 2) 타임스케일 원복
            Time.timeScale = 1f;

            // 3) 메인 메뉴로 페이드 이동
            var fade = FadeManager.Instance;
            if (fade != null)
            {
                fade.FadeToSceneAsync(mainMenuSceneIndex);
            }
        }
        
        private void ShowMapUI()
        {
            if (victoryPanel != null) victoryPanel.SetActive(false);
            if (defeatPanel != null) defeatPanel.SetActive(false);

            var fade = FadeManager.Instance;
            var mapManager = MapManager.Instance;

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
                // 2) 지금 맵 클리어 처리 (다음 노드 해금)
                mapManager.CompleteCurrentMap();

                // 3) 현재 챕터 맵 UI 다시 띄우기 (밑에서 위로 슬라이드)
                mapManager.ShowCurrentChapterRoot();

                // 4) 다시 페이드 아웃으로 전환
                fade.FadeOut();
            });
        }
    }
}