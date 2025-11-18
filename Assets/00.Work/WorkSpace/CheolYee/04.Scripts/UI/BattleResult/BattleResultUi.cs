using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Events;
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

        private void ShowGameOverUI()
        {
            
        }

        private void ShowMapUI()
        {
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
    }
}