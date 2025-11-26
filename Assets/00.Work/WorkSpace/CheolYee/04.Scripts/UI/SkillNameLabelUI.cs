using _00.Work.Scripts.Managers;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.UI
{
    public class SkillNameLabelUI : MonoSingleton<SkillNameLabelUI>
    {
        [Header("References")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProUGUI skillNameText;
        [SerializeField] private RectTransform root; // 패널 루트 (이동할 것)

        [Header("Timing")]
        [SerializeField] private float enterDuration = 0.25f;
        [SerializeField] private float stayDuration = 0.8f;
        [SerializeField] private float exitDuration = 0.3f;

        [Header("Move Offsets")]
        [Tooltip("오른쪽에서 등장할 때, 중앙 기준으로 얼마나 오른쪽에서 시작할지")]
        [SerializeField] private float enterOffsetX = 600f;

        [Tooltip("화면 중앙에서 약간 왼쪽으로 미끄러질 거리")]
        [SerializeField] private float driftOffsetX = 50f;

        [Tooltip("왼쪽으로 사라질 때, 중앙 기준으로 얼마나 왼쪽까지 보낼지")]
        [SerializeField] private float exitOffsetX = 600f;

        private Tween _currentTween;
        private Vector2 _originAnchoredPos;

        protected override void Awake()
        {
            base.Awake();
            if (Instance != this) return;

            if (root == null)
                root = GetComponent<RectTransform>();

            if (canvasGroup == null && root != null)
                canvasGroup = root.GetComponent<CanvasGroup>();

            if (root != null)
                _originAnchoredPos = root.anchoredPosition;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 스킬 이름을 오른쪽에서 등장 → 중앙 → 왼쪽으로 퇴장하는 연출로 보여준다.
        /// </summary>
        public void ShowSkillName(string skillName)
        {
            if (canvasGroup == null || skillNameText == null || root == null)
                return;

            // 진행 중인 트윈 정리
            _currentTween?.Kill();

            skillNameText.text = skillName;

            Vector2 centerPos = _originAnchoredPos;                               // 화면 중앙 위치
            Vector2 startPos  = centerPos + new Vector2(enterOffsetX, 0f);        // 오른쪽 밖
            Vector2 driftPos  = centerPos + new Vector2(-driftOffsetX, 0f);       // 중앙보다 살짝 왼쪽
            Vector2 endPos    = centerPos + new Vector2(-exitOffsetX, 0f);        // 왼쪽 밖

            root.anchoredPosition = startPos;
            canvasGroup.gameObject.SetActive(true);
            canvasGroup.alpha = 0f;

            var seq = DOTween.Sequence();

            seq.Append(root.DOAnchorPos(centerPos, enterDuration)
                .SetEase(Ease.OutCubic));
            seq.Join(canvasGroup.DOFade(1f, enterDuration));

            seq.Append(root.DOAnchorPos(driftPos, stayDuration)
                .SetEase(Ease.Linear));

            seq.Append(root.DOAnchorPos(endPos, exitDuration)
                .SetEase(Ease.InCubic));
            seq.Join(canvasGroup.DOFade(0f, exitDuration));

            seq.OnComplete(() =>
            {
                canvasGroup.gameObject.SetActive(false);
                root.anchoredPosition = centerPos;
                _currentTween = null;
            });

            _currentTween = seq;
        }
    }
}
