using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 드래그 가능한 캐릭터 아이콘 UI 제어 스크립트
/// </summary>
public class DragAbleUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Transform Canvas { get; private set; } // 가장 상단에 위치한 캔버스
    public Transform previousParent;              // 드래그 시작 전의 부모 객체 저장
    private RectTransform _rect;                  // 현재 RectTransform
    private CanvasGroup _canvasGroup;             // 투명도 및 상호작용 제어용

    private void Awake()
    {
        Canvas = FindFirstObjectByType<Canvas>().transform; // 첫 번째 캔버스 찾기
        _rect = GetComponent<RectTransform>();              // RectTransform 가져오기
        _canvasGroup = GetComponent<CanvasGroup>();         // CanvasGroup 가져오기
    }

    // 드래그 시작 시 호출
    public void OnBeginDrag(PointerEventData eventData)
    {
        previousParent = transform.parent;   // 기존 부모 저장
        transform.SetParent(Canvas);         // 최상위 캔버스로 부모 변경
        transform.SetAsLastSibling();        // 다른 UI 위로 올리기/
                                             // SetAsLastSibling(); => 하이라키 창에서 가장 하단으로 내리기

        _canvasGroup.alpha = 0.5f;           // 반투명하게 표시 
        _canvasGroup.blocksRaycasts = false; // 충돌을 비활성화
    }

    // 드래그 중 위치 갱신
    public void OnDrag(PointerEventData eventData)
    {
        _rect.position = eventData.position; // 마우스 위치로 이동
    }

    // 드래그 종료 시 호출
    public void OnEndDrag(PointerEventData eventData)
    {
        // 드롭할 대상이 없으면 원래 위치로 복귀
        if (transform.parent == Canvas)
        {
            transform.SetParent(previousParent);
            _rect.position = previousParent.GetComponent<RectTransform>().position;
        }
        _canvasGroup.alpha = 1f;             // 다시 불투명하게
        _canvasGroup.blocksRaycasts = true;  // 상호작용 복원
    }
}