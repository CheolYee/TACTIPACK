using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.UI
{
    public class ButtonClickTween : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target; // 비워두면 자기 자신

        [Header("Tween Settings")]
        [SerializeField] private float pressedScale = 0.9f; // 얼마나 눌릴지 (1보다 작게)
        [SerializeField] private float duration = 0.15f;    // 전체 왕복 시간 (들어갔다 나오는 시간)
        [SerializeField] private Ease ease = Ease.OutQuad;
        [SerializeField] private bool useUnscaledTime = true;

        private Button _button;
        private Tween _tween;
        private Vector3 _originalScale;

        private void Awake()
        {
            _button = GetComponent<Button>();
            if (target == null)
                target = transform;

            _originalScale = target.localScale;

            if (_button != null)
                _button.onClick.AddListener(PlayTween);
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(PlayTween);

            _tween?.Kill();
        }

        public void PlayTween()
        {
            if (target == null) return;

            // 기존 트윈 정리
            _tween?.Kill();
            target.localScale = _originalScale;

            float half = duration * 0.5f;

            _tween = target
                .DOScale(_originalScale * pressedScale, half)
                .SetEase(ease)
                .SetUpdate(useUnscaledTime)
                .OnComplete(() =>
                {
                    target
                        .DOScale(_originalScale, half)
                        .SetEase(ease)
                        .SetUpdate(useUnscaledTime);
                });
        }
    }
}