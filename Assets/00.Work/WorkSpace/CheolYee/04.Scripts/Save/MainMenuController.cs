using _00.Work.Resource.Scripts.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Save
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button quitButton;

        [Header("Scene Index")]
        [SerializeField] private int characterSelectSceneIndex = 1; // 새로하기 → 캐릭터 선택 씬
        [SerializeField] private int battleSceneIndex = 2; // 이어하기 → 전투(맵) 씬

        private void Awake()
        {
            // 버튼 리스너 연결
            if (newGameButton != null)
                newGameButton.onClick.AddListener(OnClickNewGame);

            if (continueButton != null)
                continueButton.onClick.AddListener(OnClickContinue);

            if (quitButton != null)
                quitButton.onClick.AddListener(OnClickQuit);
        }

        private void Start()
        {
            SoundManager.Instance.PlayBgm(BgmId.Title);
            
            // 세이브가 없으면 이어하기 비활성화
            bool hasSave = SaveManager.Instance != null && SaveManager.Instance.HasAnySave();

            if (continueButton != null)
                continueButton.interactable = hasSave;
        }

        private void OnClickNewGame()
        {
            newGameButton.interactable = false;
            // 모든 세이브 삭제 (완전 새 시작)
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.DeleteAllSaves();
            }

            // 캐릭터 선택 씬으로
            LoadSceneWithFade(characterSelectSceneIndex);
        }

        private void OnClickContinue()
        {
            continueButton.interactable = false;
            if (SaveManager.Instance == null || !SaveManager.Instance.HasAnySave())
            {
                // 혹시 버튼이 잘못 눌렸을 때 대비
                Debug.LogWarning("[MainMenu] 이어하기 세이브가 없습니다.");
                return;
            }

            // 전투/맵 씬으로 바로 이동
            LoadSceneWithFade(battleSceneIndex);
        }

        private void OnClickQuit()
        {
            quitButton.interactable = false;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void LoadSceneWithFade(int sceneIndex)
        {
            FadeManager.Instance.FadeToSceneAsync(sceneIndex);
        }
    }
}