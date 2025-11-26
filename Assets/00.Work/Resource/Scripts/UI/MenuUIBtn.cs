using _00.Work.Resource.Scripts.Managers;
using _00.Work.Scripts.Managers;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Save;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using FadeManager = _00.Work.Resource.Scripts.Managers.FadeManager;

namespace _00.Work.Resource.Scripts.UI
{
    public class MenuUIBtn : MonoBehaviour
    {
        [SerializeField] private int mainMenuSceneIndex;
        [SerializeField] private GameObject menu;
        [SerializeField] private Button mainButton;
        [SerializeField] private bool exitToMainMenu = true; 
        public Slider bgmSlider;
        public Slider sfxSlider;
        
        private bool _isPressedEsc;

        private void Start()
        {
            bgmSlider.value = SoundManager.Instance.GetBGMVolume();
            sfxSlider.value = SoundManager.Instance.GetSfxVolume();

            bgmSlider.onValueChanged.AddListener((v) => SoundManager.Instance.SetBgmVolume(v));
            sfxSlider.onValueChanged.AddListener((v) => SoundManager.Instance.SetSfxVolume(v));
            
            menu.SetActive(false);
            _isPressedEsc = false;
        }

        private void Update()
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (_isPressedEsc)
                {
                    ContinueButton();
                }
                else
                {
                    MainMenu();
                }
            }
        }

        public void MainMenu()
        {
            SoundManager.Instance.PlaySfx(SfxId.UiClick);
            _isPressedEsc = true;

            menu.SetActive(true);

            Time.timeScale = 0f;
        }
        
        public void ContinueButton()
        {
            SoundManager.Instance.PlaySfx(SfxId.UiConfirm);
            _isPressedEsc = false;
            
            Time.timeScale = 1f;

            if (mainButton != null)
                mainButton.gameObject.SetActive(true);

            menu.SetActive(false);
        }

        public void ExitButton()
        {
            SoundManager.Instance.PlaySfx(SfxId.UiConfirm);

            if (!exitToMainMenu)
            {
                Time.timeScale = 1f;
                FadeManager.Instance?.FadeToSceneAsync(mainMenuSceneIndex);
                return;
            }

            var saver = GameSaveController.Instance;
            if (saver != null)
            {
                saver.SaveAll();
            }

            Time.timeScale = 1f;

            FadeManager.Instance?.FadeToSceneAsync(mainMenuSceneIndex);
        }
    }
}
