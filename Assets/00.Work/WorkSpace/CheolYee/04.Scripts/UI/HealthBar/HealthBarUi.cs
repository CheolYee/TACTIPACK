using _00.Work.WorkSpace.CheolYee._04.Scripts.Agents;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Managers;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.UI.HealthBar
{
    public class HealthBarUi : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image hpFillImage;
        [SerializeField] private Image damageFillImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private Canvas healthBarCanvas;
        
        [Header("Fade")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float fadeDuration = 0.4f;
        
        [Header("HP Tween")]
        [SerializeField] private float hpTweenDuration = 0.2f;      // 앞바 속도
        [SerializeField] private float damageTweenDuration = 0.5f;   // 뒷바 속도
        [SerializeField] private float damageDelay = 0.1f;    
        
        private AgentHealth _health;
        private Agent _owner;
        private Tween _fadeTween;
        private Tween _hpTween;
        private Tween _damageTween;

        private void Awake()
        {
            if (!healthBarCanvas)
                healthBarCanvas.worldCamera = Camera.main;
            
            if (hpFillImage != null)
                hpFillImage.fillAmount = 1f;
            
            if (damageFillImage != null)
                damageFillImage.fillAmount = 1f;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
        }

        private void Start()
        {
            HudManager.Instance.Register(this);
        }

        public void Bind(Agent owner, AgentHealth health)
        {
            _owner = owner;
            _health = health;

            if (hpFillImage != null && _health != null)
            {
                float max = _health.MaxHealth;
                float current = _health.CurrentHealth;
                hpFillImage.fillAmount = max > 0 ? current / max : 0f;
            }

            _health.OnInitHealth += HandleHealthChange;
            _health.OnHealthChange += HandleHealthChange;
            _health.OnDeath += HandleDeath;
        }

        public void SetName(string charName)
        {
            if (nameText != null) nameText.text = charName;
        }

        private void HandleDeath()
        {
            gameObject.SetActive(false);
        }

        private void HandleHealthChange(float prevHealth, float currentHealth, float maxHealth)
        {
            if (hpFillImage == null || maxHealth <= 0f) return;

            float prevRatio = Mathf.Clamp01(prevHealth / maxHealth);
            float currentRatio = Mathf.Clamp01(currentHealth / maxHealth);

            //트윈 정리
            if (_hpTween != null && _hpTween.IsActive())
            {
                _hpTween.Kill();
                _hpTween = null;
            }

            if (_damageTween != null && _damageTween.IsActive())
            {
                _damageTween.Kill();
                _damageTween = null;
            }

            // 데미지 받은 경우
            if (currentHealth < prevHealth)
            {
                _hpTween = hpFillImage
                    .DOFillAmount(currentRatio, hpTweenDuration)
                    .SetEase(Ease.OutQuad);

                if (damageFillImage != null)
                {
                    //혹시 값이 틀어져 있으면 이전 비율로 맞춰줌
                    damageFillImage.fillAmount = prevRatio;

                    _damageTween = damageFillImage
                        .DOFillAmount(currentRatio, damageTweenDuration)
                        .SetDelay(damageDelay)
                        .SetEase(Ease.OutQuad);
                }
            }
            else //회복
            {
                _hpTween = hpFillImage
                    .DOFillAmount(currentRatio, hpTweenDuration * 0.8f)
                    .SetEase(Ease.OutQuad);

                if (damageFillImage != null)
                {
                    _damageTween = damageFillImage
                        .DOFillAmount(currentRatio, damageTweenDuration)
                        .SetEase(Ease.OutQuad);
                }
            }
        }
        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void Unsubscribe()
        {
            if (_health == null) return;

            _health.OnInitHealth -= HandleHealthChange;
            _health.OnHealthChange -= HandleHealthChange;
            _health.OnDeath -= HandleDeath;
            _health = null;
        }

        public void SetVisibleImmediate(bool visible)
        {
            if (canvasGroup == null) return;

            if (_fadeTween != null && _fadeTween.IsActive())
            {
                _fadeTween.Kill();
                _fadeTween = null;
            }

            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
        }
        
        public void SetVisible(bool visible)
        {
            FadeVisible(visible, fadeDuration);
        }
        
        private void FadeVisible(bool visible, float duration)
        {
            if (canvasGroup == null) return;

            if (_fadeTween != null && _fadeTween.IsActive())
            {
                _fadeTween.Kill();
                _fadeTween = null;
            }

            float targetAlpha = visible ? 1f : 0f;

            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;

            if (duration <= 0f)
            {
                canvasGroup.alpha = targetAlpha;
                return;
            }

            _fadeTween = canvasGroup
                .DOFade(targetAlpha, duration)
                .SetUpdate(true);
        }
        
    }
}