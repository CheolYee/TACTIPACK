using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 드래그 가능한 캐릭터 아이콘 UI 제어 스크립트
/// </summary>
public class DragAbleUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{

    public Transform Canvas { get; private set; } //가장 상단에 위치한 캔바스
    public Transform previousParent; // 이전 부모를 저장
    private RectTransform _rect; //현재 위치
    private CanvasGroup _canvasGroup;


    private void Awake()
    {
        Canvas = FindFirstObjectByType<Canvas>().transform;
        _rect = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        previousParent = transform.parent; //이전 부모에 저장

        transform.SetParent(Canvas); //부모 변경
        transform.SetAsLastSibling();//하이라키 창 가장 마지막으로 배치

        _canvasGroup.alpha = 0.5f;  
        _canvasGroup.blocksRaycasts = false;
    }
    public void OnDrag(PointerEventData eventData)
    {
        _rect.position = eventData.position;
    }
    public void OnEndDrag(PointerEventData eventData)
    {
       if(transform.parent == Canvas)
        {
            transform.SetParent(previousParent);   
            _rect.position = previousParent.GetComponent<RectTransform>().position;
        }
            _canvasGroup.alpha = 1f;
        _canvasGroup.blocksRaycasts = true;
    }
}
