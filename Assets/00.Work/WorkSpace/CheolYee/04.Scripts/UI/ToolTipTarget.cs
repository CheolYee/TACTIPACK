using _00.Work.WorkSpace.CheolYee._04.Scripts.Managers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.UI
{
    public class TooltipTarget : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
    {
        [Header("Tooltip Text")]
        [TextArea] public string title;
        [TextArea(3, 5)] public string body;

        [Header("Settings")]
        [SerializeField] private float showDelay = 1.0f;

        private bool _hover;
        private float _hoverTimer;
        private bool _shown;

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hover = true;
            _hoverTimer = 0f;
            _shown = false;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hover = false;
            _shown = false;
            _hoverTimer = 0f;
            ToolTipManager.Instance?.Hide();
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            // 이미 떠 있는 상태면 마우스 이동에 따라 위치 갱신
            if (_shown && ToolTipManager.Instance != null)
            {
                ToolTipManager.Instance.UpdatePosition(eventData.position);
            }
        }

        private void Update()
        {
            if (!_hover) return;

            _hoverTimer += Time.unscaledDeltaTime;
            if (!_shown && _hoverTimer >= showDelay)
            {
                _shown = true;
                ToolTipManager.Instance?.Show(title, body, Input.mousePosition);
            }

            // 만약 Move 이벤트가 안 들어오는 환경일 수도 있으니 안전하게 한 번 더
            if (_shown && ToolTipManager.Instance != null)
            {
                ToolTipManager.Instance.UpdatePosition(Input.mousePosition);
            }
        }

        /// <summary>
        /// 런타임에 텍스트를 바꾸고 싶을 때 사용(So에서 데이터 받아올 때 등)
        /// </summary>
        public void SetText(string newTitle, string newBody)
        {
            Debug.Log($"SetText: {newTitle} -> {newBody}");
            title = newTitle;
            body = newBody;

            if (_shown && ToolTipManager.Instance != null)
            {
                ToolTipManager.Instance.Show(title, body, Input.mousePosition);
            }
        }
    }
}
