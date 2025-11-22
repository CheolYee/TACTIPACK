using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.UI.Turn
{
    public class BattleStartButtonController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Button startButton;
        [SerializeField] private RectTransform buttonRoot; //버튼이 달린 패널/자기 RectTransform
        [SerializeField] private CanvasGroup inputBlocker; //화면 전체에 얹는 클릭 막기용 (알파 0도 상관 없음)

        [Header("Tween Settings")]
        [SerializeField] private float moveDistanceX = 400f; //오른쪽으로 나갈 거리
        [SerializeField] private float moveDuration = 0.3f;

        private Vector2 _shownPos; //버튼이 화면에 있을 때 위치
        private bool _initialized;
        private bool _busy; // true면 다시 못 누르도록
        private Tween _moveTween;

        private void Awake()
        {
            if (startButton == null)
                startButton = GetComponent<Button>();

            if (buttonRoot == null)
                buttonRoot = startButton != null
                    ? startButton.GetComponent<RectTransform>()
                    : GetComponent<RectTransform>();

            if (buttonRoot != null)
                _shownPos = buttonRoot.anchoredPosition;

            if (startButton != null)
                startButton.onClick.AddListener(OnClickStart);

            SetupBlocker(false);
        }

        private void OnDestroy()
        {
            if (startButton != null)
                startButton.onClick.RemoveListener(OnClickStart);

            _moveTween?.Kill();
        }

        private void SetupBlocker(bool enable)
        {
            if (inputBlocker == null) return;

            inputBlocker.gameObject.SetActive(enable);
            inputBlocker.alpha = 0f; // 안 보여도 됨
            inputBlocker.blocksRaycasts = enable;
            inputBlocker.interactable = enable;
        }

        private void OnClickStart()
        {
            if (_busy) return;

            var panel = TurnUiContainerPanel.Instance;
            if (panel == null)
            {
                Debug.LogWarning("[BattleStartButtonController] TurnUiContainerPanel이 없습니다.");
                return;
            }

            var coroutine = panel.StartTurnSequence();
            if (coroutine == null)
            {
                Debug.LogWarning("[BattleStartButtonController] 실행할 턴 시퀀스가 없습니다.");
                return;
            }

            StartCoroutine(StartFlow(coroutine));
        }

        private IEnumerator StartFlow(Coroutine battleCoroutine)
        {
            _busy = true;
            if (startButton != null)
                startButton.interactable = false;

            SetupBlocker(true); // 바인딩/설치 못 하게 막기

            // 1) 버튼 오른쪽으로 빠지기
            if (buttonRoot != null)
            {
                _moveTween?.Kill();
                Vector2 hiddenPos = _shownPos + new Vector2(moveDistanceX, 0f);

                _moveTween = buttonRoot.DOAnchorPos(hiddenPos, moveDuration)
                    .SetEase(Ease.OutCubic);
                yield return _moveTween.WaitForCompletion();
            }

            // 2) 턴 시퀀스 끝날 때까지 대기 (플레이어 턴 + 에너미 턴)
            yield return battleCoroutine;

            // 3) 버튼 다시 제자리로 들어오기
            if (buttonRoot != null)
            {
                _moveTween?.Kill();

                // 혹시 위치가 어긋나 있었어도, 항상 이 위치로
                _moveTween = buttonRoot.DOAnchorPos(_shownPos, moveDuration)
                    .SetEase(Ease.OutCubic);
                yield return _moveTween.WaitForCompletion();
            }

            SetupBlocker(false);

            if (startButton != null)
                startButton.interactable = true;

            _busy = false;
        }
    }
}
