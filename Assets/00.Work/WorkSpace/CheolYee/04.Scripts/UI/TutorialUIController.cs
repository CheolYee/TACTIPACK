using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.UI
{
    
    [System.Serializable]
    public class TutorialGroup
    {
        public string id;
        public string displayName;
        public List<Sprite> pages;
    }

    public class TutorialUIController : MonoBehaviour
    {
        [Header("Panel & UI")] [SerializeField]
        private GameObject tutorialPanel;

        [SerializeField] private Image tutorialImage;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button prevButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI categoryTitleText; // 선택사항
        [SerializeField] private TextMeshProUGUI indexText; // 선택사항

        [Header("Tutorial Groups")] [SerializeField]
        private List<TutorialGroup> groups = new List<TutorialGroup>();

        [Header("Tween Settings")] [SerializeField]
        private float slideDistance = 300f;

        [SerializeField] private float slideDuration = 0.25f;
        [SerializeField] private Ease slideEase = Ease.OutQuad;

        private int _currentGroupIndex;
        private int _currentPageIndex;
        private RectTransform _imageRect;
        private Vector2 _initialPos;
        private bool _isAnimating;

        private void Awake()
        {
            _imageRect = tutorialImage.rectTransform;
            _initialPos = _imageRect.anchoredPosition;

            if (tutorialPanel != null)
                tutorialPanel.SetActive(false);

            if (nextButton != null) nextButton.onClick.AddListener(OnClickNext);
            if (prevButton != null) prevButton.onClick.AddListener(OnClickPrev);
            if (closeButton != null) closeButton.onClick.AddListener(CloseTutorial);
            if (indexText != null)
                indexText.text = string.Empty;
        }

        /// <summary>
        /// 특정 그룹 인덱스를 열고 싶을 때 사용 (버튼에서 직접 index 넘기기)
        /// </summary>
        public void OpenTutorialByIndex(int groupIndex)
        {
            if (groupIndex < 0 || groupIndex >= groups.Count)
            {
                Debug.LogWarning("Invalid tutorial group index");
                return;
            }

            _currentGroupIndex = groupIndex;
            _currentPageIndex = 0;

            tutorialPanel.SetActive(true);
            ShowCurrentPage(withTween: false);
        }

        /// <summary>
        /// string id로도 열 수 있게 (원하면)
        /// </summary>
        public void OpenTutorialById(string id)
        {
            int index = groups.FindIndex(g => g.id == id);
            if (index == -1)
            {
                Debug.LogWarning($"Tutorial group not found: {id}");
                return;
            }

            OpenTutorialByIndex(index);
        }

        public void CloseTutorial()
        {
            tutorialPanel.SetActive(false);
        }

        private TutorialGroup CurrentGroup => groups[_currentGroupIndex];

        private void ShowCurrentPage(bool withTween)
        {
            if (CurrentGroup.pages == null || CurrentGroup.pages.Count == 0) return;
            if (_currentPageIndex < 0 || _currentPageIndex >= CurrentGroup.pages.Count) return;
            
            tutorialImage.sprite = CurrentGroup.pages[_currentPageIndex];
            UpdateButtons();
            UpdateCategoryTitle();

            _imageRect.DOKill();

            if (!withTween)
            {
                _imageRect.anchoredPosition = _initialPos;
                return;
            }

            _isAnimating = true;
            _imageRect.anchoredPosition = _initialPos + Vector2.right * slideDistance;
            _imageRect.DOAnchorPos(_initialPos, slideDuration)
                .SetEase(slideEase)
                .OnComplete(() => _isAnimating = false);
        }

        private void UpdateCategoryTitle()
        {
            if (categoryTitleText == null) return;
            categoryTitleText.text = CurrentGroup.displayName;
        }

        private void UpdateButtons()
        {
            if (prevButton != null)
                prevButton.interactable = _currentPageIndex > 0;

            if (nextButton != null)
                nextButton.interactable = _currentPageIndex < CurrentGroup.pages.Count - 1;
            
            if (indexText != null)
                indexText.text = $"{_currentPageIndex + 1} / {CurrentGroup.pages.Count}";
        }

        public void OnClickNext()
        {
            if (_isAnimating) return;
            if (_currentPageIndex >= CurrentGroup.pages.Count - 1) return;

            SlideToPage(_currentPageIndex + 1, moveLeft: true);
        }

        public void OnClickPrev()
        {
            if (_isAnimating) return;
            if (_currentPageIndex <= 0) return;

            SlideToPage(_currentPageIndex - 1, moveLeft: false);
        }

        private void SlideToPage(int newPageIndex, bool moveLeft)
        {
            _isAnimating = true;
            _imageRect.DOKill();

            float dir = moveLeft ? -1f : 1f;
            Vector2 targetPos = _initialPos + Vector2.right * (dir * slideDistance);

            // 현재 페이지가 왼쪽/오른쪽으로 빠져나감
            _imageRect.DOAnchorPos(targetPos, slideDuration)
                .SetEase(slideEase)
                .OnComplete(() =>
                {
                    _currentPageIndex = newPageIndex;

                    tutorialImage.sprite = CurrentGroup.pages[_currentPageIndex];
                    UpdateButtons();
                    UpdateCategoryTitle();

                    // 새 페이지 반대쪽에서 들어오기
                    _imageRect.anchoredPosition = _initialPos - Vector2.right * (dir * slideDistance);
                    _imageRect.DOAnchorPos(_initialPos, slideDuration)
                        .SetEase(slideEase)
                        .OnComplete(() => _isAnimating = false);
                });
        }
    }
}