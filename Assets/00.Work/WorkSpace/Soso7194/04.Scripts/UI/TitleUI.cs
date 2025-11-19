using _00.Work.Resource.Scripts.Managers;
using _00.Work.Scripts.Managers;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _00.Work.WorkSpace.Soso7194._04.Scripts.UI
{
    public class TitleUI : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private GameObject[] mainButtons;
        [SerializeField] private GameObject[] gameButtons;
        
        [Header("Panel")]
        [SerializeField] private GameObject settingPanel;
        
        [Header("ScrollBar")]
        [SerializeField] private Slider sfxBar;
        [SerializeField] private Slider bgmBar;
        
        [Header("Text")]
        [SerializeField] private TextMeshProUGUI titleText;

        private bool _isSetting = false;
        private bool _isStarting = false;

        private void Start()
        {
            sfxBar.value = SoundManager.Instance.GetSfxVolume();
            bgmBar.value = SoundManager.Instance.GetBGMVolume();
            
            Sequence seq = DOTween.Sequence();

            seq.Append(titleText.transform.DOMove(titleText.transform.position + new Vector3(0,-175,0), 0.3f));
            foreach (var button in mainButtons)
            {
                seq.Append(button.transform.DOMove(button.transform.position + new Vector3(250,0,0), 0.3f));
            }
        }

        public void StartGame()
        {
            _isStarting = !_isStarting;
            Sequence seq = DOTween.Sequence();

            if (_isStarting)
            {
                foreach (var button in gameButtons)
                {
                    seq.Append(button.transform.DOMove(button.transform.position + new Vector3(400,0,0), 0.3f));
                }
            }
            else
            {
                foreach (var button in gameButtons)
                {
                    seq.Append(button.transform.DOMove(button.transform.position + new Vector3(-400,0,0), 0.3f));
                }
            }
        }

        public void NewGame()
        {
            Debug.Log("New Game");
            // 대충 저장되어 있는 JSON 지우고 생성
            FadeManager.Instance.FadeToScene(1);
        }

        public void LoadGame()
        {
            Debug.Log("Load Game");
            // 대충 저장되어 있는 JSON 불러오기
            /* if (// JSON 파일이 있으면)
            {
                FadeManager.Instance.FadeToScene(1);
            }
            else
            {
                // Debug.LogWarning("Can't Load JSON File");
            } */
        }

        public void Setting()
        {
            _isSetting = !_isSetting;
            Sequence seq = DOTween.Sequence();

            if (_isSetting == true)
            {
                seq.Append(settingPanel.transform.DOMove(settingPanel.transform.position + new Vector3(-300,0,0), 0.3f));
            }
            else
            {
                seq.Append(settingPanel.transform.DOMove(settingPanel.transform.position + new Vector3(300,0,0), 0.3f));
            }
        }

        public void Quit()
        {
            Debug.Log("Quit");
            Application.Quit();
        }
    }
}
