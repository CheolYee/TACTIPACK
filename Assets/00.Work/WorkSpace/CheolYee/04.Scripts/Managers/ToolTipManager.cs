using _00.Work.Scripts.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Managers
{
    public class ToolTipManager : MonoSingleton<ToolTipManager>
    {
        [Header("Refs")]
        [SerializeField] private Canvas canvas;
        [SerializeField] private RectTransform root;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI bodyText;

        [Header("Settings")]
        [SerializeField] private Vector2 screenOffset = new(16f, -16f);
        [SerializeField] private Vector2 screenPadding = new(8f, 8f);
        
        private bool _visible;
        
        protected override void Awake()
        {
            base.Awake();
            if (Instance != this) return;

            if (canvas == null)
                canvas = GetComponentInParent<Canvas>();

            HideImmediate();
        }

        public void HideImmediate()
        {
            _visible = false;
            if (root != null)
                root.gameObject.SetActive(false);
        }
        
        public void Show(string title, string body, Vector2 screenPos)
        {
            if (root == null || title == null || body == null) return;

            if (titleText != null)
                titleText.text = title;

            if (bodyText != null)
            {
                bodyText.richText = true; //색 태그 등 사용 가능
                bodyText.text = body;
            }

            root.gameObject.SetActive(true);
            _visible = true;

            //레이아웃 갱신 한 프레임 이후에 위치를 다시 잡기
            LayoutRebuilder.ForceRebuildLayoutImmediate(root);
            SetPosition(screenPos);
        }

        public void Hide()
        {
            if (!_visible) return;
            _visible = false;
            if (root != null)
                root.gameObject.SetActive(false);
        }

        /// <summary>
        /// 마우스 따라다니게 쓰는 용도
        /// </summary>
        public void UpdatePosition(Vector2 screenPos)
        {
            if (!_visible) return;
            SetPosition(screenPos);
        }
        
        private void SetPosition(Vector2 screenPos)
        {
            if (canvas == null || root == null) return;

            RectTransform canvasRect = canvas.transform as RectTransform;
            if (canvasRect == null) return;

            // ★ RenderMode에 따라 카메라 결정
            Camera uiCamera = null;
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera ||
                canvas.renderMode == RenderMode.WorldSpace)
            {
                uiCamera = canvas.worldCamera;
            }
            // ScreenSpaceOverlay면 null 그대로

            // 1) 스크린 → 캔버스 로컬
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, screenPos, uiCamera, out Vector2 localPoint);

            // 2) 마우스 기준 + 오프셋
            Vector2 anchored = localPoint + screenOffset;

            // 3) 현재 툴팁 크기
            Vector2 size = root.rect.size;
            Vector2 pivot = root.pivot;

            // 4) 캔버스 경계 (로컬 좌표)
            Rect canvasBounds = canvasRect.rect;

            float minX = canvasBounds.xMin + screenPadding.x;
            float maxX = canvasBounds.xMax - screenPadding.x;
            float minY = canvasBounds.yMin + screenPadding.y;
            float maxY = canvasBounds.yMax - screenPadding.y;

            // 툴팁이 실제로 차지하는 영역 계산
            float left   = anchored.x - size.x * pivot.x;
            float right  = anchored.x + size.x * (1f - pivot.x);
            float bottom = anchored.y - size.y * pivot.y;
            float top    = anchored.y + size.y * (1f - pivot.y);

            // 5) 좌우 클램프
            if (left < minX) anchored.x += (minX - left);
            if (right > maxX) anchored.x -= (right - maxX);

            // 6) 상하 클램프
            if (bottom < minY) anchored.y += (minY - bottom);
            if (top > maxY) anchored.y -= (top - maxY);

            root.anchoredPosition = anchored;
        }
    }
}