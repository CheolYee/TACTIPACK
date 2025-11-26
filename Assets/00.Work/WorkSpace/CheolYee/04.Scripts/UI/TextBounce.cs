using DG.Tweening;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.UI
{
    public class TextBounce : MonoBehaviour
    {
        [SerializeField] private float amplitude = 10f;
        [SerializeField] private float duration = 0.5f;

        private RectTransform _rect;
        private Tween _tween;
        private Vector2 _startPos;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
            _startPos = _rect.anchoredPosition;
        }

        private void OnEnable()
        {
            StartBounce();
        }

        private void OnDisable()
        {
            if (_tween != null && _tween.IsActive())
            {
                _tween.Kill();
            }

            if (_rect != null)
            {
                _rect.anchoredPosition = _startPos;
            }
        }

        private void StartBounce()
        {
            if (_tween != null && _tween.IsActive())
            {
                _tween.Kill();
            }

            _rect.anchoredPosition = _startPos;

            _tween = _rect
                .DOAnchorPosY(_startPos.y + amplitude, duration)
                .SetLoops(-1, LoopType.Yoyo)       // 무한 반복
                .SetEase(Ease.InOutSine);          // 부드럽게 위아래
        }
    }
}