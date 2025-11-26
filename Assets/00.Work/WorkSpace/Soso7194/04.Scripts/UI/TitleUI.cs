using _00.Work.Resource.Scripts.Managers;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _00.Work.WorkSpace.Soso7194._04.Scripts.UI
{
    public class TitleUI : MonoBehaviour
    {
        [Header("Manager")]
        [SerializeField] private GameObject fadeManager;
        
        [Header("Buttons")]
        [SerializeField] private GameObject[] mainButtons;
        [SerializeField] private GameObject[] gameButtons;
        
        [Header("Text")]
        [SerializeField] private RectTransform titleText;

        private bool _isSetting;
        private bool _isStarting;
        private bool _isGameButtonsAnimating;   // 🔹 게임 버튼 트윈 중인지 여부

        private void StartClickSound()
        {
            SoundManager.Instance?.PlaySfx(SfxId.UiClick);
        }
        
        private void StartClickSoundConfirm()
        {
            SoundManager.Instance?.PlaySfx(SfxId.UiConfirm);
        }

        // 🔹 공통 버튼 on/off 함수
        private void SetButtonsInteractable(GameObject[] buttons, bool interactable)
        {
            if (buttons == null) return;

            foreach (var obj in buttons)
            {
                if (obj == null) continue;
                var btn = obj.GetComponent<Button>();
                if (btn == null) continue;

                btn.interactable = interactable;
            }
        }

        private void Start()
        {
            Sequence seq = DOTween.Sequence();

            foreach (var button in mainButtons)
            {
                button.GetComponent<Button>().onClick.AddListener(StartClickSound);
            }

            foreach (var button in gameButtons)
            {
                button.GetComponent<Button>().onClick.AddListener(StartClickSoundConfirm);
            }

            // 🔹 메인 버튼들 애니메이션 동안 클릭 막기
            SetButtonsInteractable(mainButtons, false);

            // 제목 내려오는 모션
            if (titleText != null)
            {
                Vector2 titleStart = titleText.anchoredPosition;
                seq.Append(
                    titleText.DOAnchorPos(
                        titleStart + new Vector2(0f, -400f),
                        0.3f
                    )
                );
            }

            // 메인 버튼들 슬라이드
            foreach (var button in mainButtons)
            {
                if (button == null) continue;

                var rt = button.GetComponent<RectTransform>();
                if (rt == null) continue;

                Vector2 start = rt.anchoredPosition;

                seq.Append(
                    rt.DOAnchorPos(
                        start + new Vector2(650f, 0f),
                        0.3f
                    )
                );
            }

            // 🔹 트윈 끝났을 때 다시 클릭 가능
            seq.OnComplete(() =>
            {
                SetButtonsInteractable(mainButtons, true);
            });
        }

        public void StartGame()
        {
            // 🔹 이미 애니 중이면 또 눌러도 무시
            if (_isGameButtonsAnimating)
                return;

            _isStarting = !_isStarting;
            _isGameButtonsAnimating = true;

            Sequence seq = DOTween.Sequence();

            // 🔹 여기서는 gameButtons의 interactable 을 건드리지 않는다!

            foreach (var button in gameButtons)
            {
                if (button == null) continue;

                var rt = button.GetComponent<RectTransform>();
                if (rt == null) continue;

                Vector2 current = rt.anchoredPosition;

                if (_isStarting)
                {
                    seq.Append(
                        rt.DOAnchorPos(
                            current + new Vector2(1000f, 0f),
                            0.3f
                        )
                    );
                }
                else
                {
                    seq.Append(
                        rt.DOAnchorPos(
                            current + new Vector2(-1000f, 0f),
                            0.3f
                        )
                    );
                }
            }

            seq.OnComplete(() =>
            {
                // 🔹 트윈 끝난 후 플래그만 해제
                _isGameButtonsAnimating = false;
            });
        }
    }
}
