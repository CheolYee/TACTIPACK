using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Events;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.UI
{
    public class TextShower : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Timing")]
        [SerializeField] private float fadeDuration = 0.2f;
        [SerializeField] private float displayDuration = 1.0f;

        private void OnEnable()
        {
            Bus<MessageEvent>.OnEvent += OnMessageEvent;
        }

        private void OnDisable()
        {
            Bus<MessageEvent>.OnEvent -= OnMessageEvent;
        }

        private void OnMessageEvent(MessageEvent evt)
        {
            Show(evt.Message);
        }

        private void Show(string message)
        {
            if (messageText == null || canvasGroup == null)
            {
                Debug.LogWarning($"InventoryMessageUI: UI reference missing. Message = {message}");
                return;
            }

            messageText.text = message;

            canvasGroup.DOKill();
            canvasGroup.gameObject.SetActive(true);
            canvasGroup.alpha = 0f;

            Sequence seq = DOTween.Sequence();
            seq.Append(canvasGroup.DOFade(1f, fadeDuration));
            seq.AppendInterval(displayDuration);
            seq.Append(canvasGroup.DOFade(0f, fadeDuration));
            seq.OnComplete(() =>
            {
                canvasGroup.gameObject.SetActive(false);
            });
        }
    }
}