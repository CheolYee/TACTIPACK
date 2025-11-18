
using System;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Events;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.UI.BattleResult
{
    public class BattleResultUi : UnityEngine.MonoBehaviour
    {
        [Header("Result Panels")]
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private GameObject defeatPanel;

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