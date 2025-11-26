using _00.Work.Resource.Scripts.Managers;
using _00.Work.Resource.Scripts.SO;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Attacks.Damages
{
    public class DamageTextUi : MonoBehaviour, IPoolable
    {
        [Header("Refs")]
        [SerializeField] private TextMeshPro text;

        [Header("Anim Settings")]
        [SerializeField] private float lifetime = 0.6f;
        [SerializeField] private float moveUpDistance = 1f;
        [SerializeField] private Vector3 worldOffset = new(0f, 1f, 0f);

        [Header("Colors")]
        [SerializeField] private Color normalColor = Color.yellow;
        [SerializeField] private Color criticalColor = Color.red;
        public Color bleedColor = new Color(0.8f, 0f, 0.8f);
        public Color burnColor = new Color(1f, 0.5f, 0f);
        public Color healColor = Color.green;  
        public Color barrierColor = Color.yellow;  

        [Header("Pool")]
        [SerializeField] private string itemName = "DamageText";
        
        private Tween _tween;
        public string ItemName => itemName;
        public GameObject GameObject => gameObject;

        private void OnDisable()
        {
            if (_tween != null && _tween.IsActive())
            {
                _tween.Kill();
                _tween = null;
            }
        }

        public void ResetItem()
        {
            if (_tween != null && _tween.IsActive())
            {
                _tween.Kill();
                _tween = null;
            }
            
            if (text != null)
            {
                var c = text.color;
                c.a = 1f;
                text.color = c;
            }

            transform.localScale = Vector3.one;
        }
        
        public void Play(float damage, bool isCritical, Vector3 worldPos,
            DamageTextKind kind = DamageTextKind.Normal)
        {
            if (text == null)
                return;

            transform.position = worldPos + worldOffset;

            text.text = Mathf.RoundToInt(damage).ToString();
            text.color = GetColor(isCritical, kind);

            if (_tween != null && _tween.IsActive())
            {
                _tween.Kill();
                _tween = null;
            }

            Vector3 startPos = transform.position;
            Vector3 endPos = startPos + Vector3.up * moveUpDistance;

            _tween = DOTween.Sequence()
                .Join(transform.DOMove(endPos, lifetime).SetEase(Ease.OutQuad))
                .Join(text.DOFade(0f, lifetime).SetEase(Ease.Linear))
                .OnComplete(() =>
                {
                    PoolManager.Instance.Push(this);
                });
        }
        
        //색깔갖고와!!!!!!!!!!
        private Color GetColor(bool isCritical, DamageTextKind kind)
        {
            switch (kind)
            {
                case DamageTextKind.Bleed:
                    return bleedColor;
                case DamageTextKind.Burn:
                    return burnColor;
                case DamageTextKind.Heal:
                    return healColor;
                case DamageTextKind.Barrier:
                    return barrierColor;
                case DamageTextKind.Normal:
                default:
                    return isCritical ? criticalColor : normalColor;
            }
        }
    }
}