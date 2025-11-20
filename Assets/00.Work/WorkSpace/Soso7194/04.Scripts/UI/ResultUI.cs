using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace _00.Work.WorkSpace.Soso7194._04.Scripts.UI
{
    public class ResultUI : MonoBehaviour
    {
        [SerializeField] private Image[] playerImages;
        [SerializeField] private TextMeshProUGUI[] playerNameText;
        [SerializeField] private TextMeshProUGUI[] playerHPText;

        private int _hp = 100;

        private void OnEnable()
        {
            foreach (var image in playerImages)
            {
                image.sprite = null;
            }

            foreach (var text in playerNameText)
            {
                text.text = "test";
            }

            foreach (var text in playerHPText)
            {
                int i = Random.Range(10, 100);
                text.text = $"{_hp} => {_hp + i}";
            }
        }
    }
}