using _00.Work.Scripts.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Managers
{
    public class BattleSpeedManager : MonoSingleton<BattleSpeedManager>
    {
        [Header("Speed Settings")]
        [SerializeField] private float normalTimeScale = 1f;
        [SerializeField] private float fastTimeScale = 2f;

        [Header("UI")]
        [SerializeField] private Button toggleButton; //2배속 버튼
        [SerializeField] private Image iconImage; //아이콘 쓰고싶으면
        [SerializeField] private Sprite normalSprite; //1배속 아이콘
        [SerializeField] private Sprite fastSprite; //2배속 아이콘

        public bool IsFast => _isFast;

        private bool _isFast;

        protected override void Awake()
        {
            base.Awake();
            if (Instance != this) return;
            Time.timeScale = normalTimeScale;
        }
        
        public void SetNormalSpeed()
        {
            SetFast(false);
        }

        private void Start()
        {
            if (toggleButton != null)
                toggleButton.onClick.AddListener(ToggleSpeed);

            UpdateVisual();
        }

        protected override void OnDestroy()
        {
            if (Instance == this)
            {
                Time.timeScale = 1f;
            }

            if (toggleButton != null)
                toggleButton.onClick.RemoveListener(ToggleSpeed);
        }

        public void ToggleSpeed()
        {
            SetFast(!_isFast);
        }

        public void SetFast(bool fast)
        {
            _isFast = fast;
            Time.timeScale = _isFast ? fastTimeScale : normalTimeScale;
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            if (iconImage != null)
            {
                if (_isFast && fastSprite != null)
                    iconImage.sprite = fastSprite;
                else if (!_isFast && normalSprite != null)
                    iconImage.sprite = normalSprite;
            }
        }
    }
}