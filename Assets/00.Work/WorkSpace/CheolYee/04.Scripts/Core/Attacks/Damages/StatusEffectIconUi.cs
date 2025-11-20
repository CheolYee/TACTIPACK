using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Attacks.Damages
{
    public class StatusEffectIconUi : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI turnText;
        
        private Tween _popTween;

        public void SetIcon(Sprite icon, int remainingTurns)
        {
            if (iconImage != null)
                iconImage.sprite = icon;

            UpdateTurn(remainingTurns, true);
        }

        public void UpdateTurn(int remainingTurns, bool playTween = true)
        {
            if (turnText != null)
                turnText.text = remainingTurns.ToString();

            if (playTween)
                PlayPopTween();
        }

        private void PlayPopTween()
        {
            _popTween?.Kill();
            
            transform.localScale = Vector3.one * 0.9f;
            _popTween = transform
                .DOScale(1f, 0.15f)
                .SetEase(Ease.OutBack, 1.1f);
        }
        private void OnDisable()
        {
            _popTween?.Kill();
            _popTween = null;
        }
        
    }
}