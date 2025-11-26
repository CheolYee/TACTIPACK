using System;
using System.Collections.Generic;
using System.Linq;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Agents;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Attacks.Damages;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Managers;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.UI.HealthBar
{
    [Serializable]
    public struct StatusIconEntry
    {
        public StatusEffectType type;
        public Sprite icon;
        public string title;
        [TextArea]
        public string description;
    }
    
    public class HealthBarUi : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image hpFillImage;
        [SerializeField] private Image damageFillImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private Canvas healthBarCanvas;
        
        [Header("Fade")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float fadeDuration = 0.4f;
        
        [Header("HP Tween")]
        [SerializeField] private float hpTweenDuration = 0.2f;      // 앞바 속도
        [SerializeField] private float damageTweenDuration = 0.5f;   // 뒷바 속도
        [SerializeField] private float damageDelay = 0.1f;
        
        [Header("Status Effects")]
        [SerializeField] private RectTransform statusEffectRoot;          // 가로 자동정렬 오브젝트
        [SerializeField] private StatusEffectIconUi statusIconPrefab;
        [SerializeField] private StatusIconEntry[] statusIconEntries;
        
        private Dictionary<StatusEffectType, StatusIconEntry> _statusIconMap;
        private readonly Dictionary<StatusEffectType, StatusEffectIconUi> _statusIcons = new();
        
        private AgentHealth _health;
        private Agent _owner;
        private StatusEffectController _statusController;
        
        private Tween _fadeTween;
        private Tween _hpTween;
        private Tween _hpTextTween;
        private Tween _damageTween;

        private void Awake()
        {
            if (healthBarCanvas != null)
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
            
            //스프라이트 매핑
            _statusIconMap = new Dictionary<StatusEffectType, StatusIconEntry>();
            if (statusIconEntries != null)
            {
                foreach (var entry in statusIconEntries)
                {
                    if (!_statusIconMap.ContainsKey(entry.type) && entry.icon != null)
                        _statusIconMap.Add(entry.type, entry);
                }
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

            _health.OnInitHealth += HandleHealthChange;
            _health.OnHealthChange += HandleHealthChange;
            _health.OnDeath += HandleDeath;
            
            HandleHealthChange(_health.CurrentHealth, _health.CurrentHealth, _health.MaxHealth);
            
            _statusController = _owner.GetCompo<StatusEffectController>();
            if (_statusController != null)
            {
                _statusController.OnStatusChanged += HandleStatusChanged;
                HandleStatusChanged(_statusController); // 초기 아이콘 세팅
            }
        }
        
        private void TryGetIconEntry(StatusEffectType type, out StatusIconEntry entry)
        {
            if (_statusIconMap != null && _statusIconMap.TryGetValue(type, out entry)) return;

            entry = default;
        }

        private void HandleStatusChanged(StatusEffectController controller)
        {
            if (statusEffectRoot == null || statusIconPrefab == null)
                return;

            IReadOnlyList<StatusEffectInstance> effects = controller.ActiveEffects;

            //더 이상 존재하지 않는 효과의 아이콘 제거
            var keysToRemove = new List<StatusEffectType>();
            foreach (var kv in _statusIcons)
            {
                bool stillExists = effects.Any(e => e.Type == kv.Key && e.RemainingTurns > 0);
                if (!stillExists)
                {
                    if (kv.Value != null)
                        Destroy(kv.Value.gameObject);

                    keysToRemove.Add(kv.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                _statusIcons.Remove(key);
            }

            //현재 효과 리스트 기준으로 아이콘 업뎃
            foreach (var effect in effects)
            {
                if (effect.RemainingTurns <= 0)
                    continue;

                if (!_statusIcons.TryGetValue(effect.Type, out var iconUi) || iconUi == null)
                {
                    iconUi = Instantiate(statusIconPrefab, statusEffectRoot);
                    _statusIcons[effect.Type] = iconUi;

                    TryGetIconEntry(effect.Type, out var entry);

                    iconUi.SetIcon(entry.icon, effect.RemainingTurns, entry.title, entry.description);
                }
                else
                {
                    iconUi.UpdateTurn(effect.RemainingTurns);
                }
            }
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
            
            if (hpText != null)
            {
                hpText.text = currentHealth.ToString("N0");
            }

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
            
            if (_statusIconMap == null) return;
            _statusController.OnStatusChanged -= HandleStatusChanged;
            _statusController = null;
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