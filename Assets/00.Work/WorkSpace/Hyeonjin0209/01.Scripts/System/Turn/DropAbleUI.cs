using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 드래그된 UI 아이콘을 드롭할 수 있는 슬롯 제어 스크립트
/// </summary>
public class DropAbleUI : MonoBehaviour, IPointerEnterHandler, IDropHandler, IPointerExitHandler
{
    private Image _image;           // 슬롯의 배경 이미지
    private RectTransform _rect;    // 슬롯의 위치 정보

    private void Awake()
    {
        _image = GetComponent<Image>();           // 이미지 컴포넌트 가져오기
        _rect = GetComponent<RectTransform>();    // RectTransform 가져오기
    }

    // 드롭 시 호출
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            // 드래그 중인 아이콘 가져오기
            DragAbleUI draggedIcon = eventData.pointerDrag.GetComponent<DragAbleUI>();

            // 드래그된 아이콘의 복사본 생성 (캔버스 위에)
            DragAbleUI copy =
                Instantiate(draggedIcon.gameObject, draggedIcon.Canvas).GetComponent<DragAbleUI>();

            copy.gameObject.name = draggedIcon.gameObject.name;   // 이름 동일하게
            copy.previousParent = draggedIcon.previousParent;      // 부모 정보 복사
            copy.OnEndDrag(eventData);                             // 드래그 종료 처리  

            // 실제 드래그된 아이콘을 이 슬롯으로 이동
            draggedIcon.transform.SetParent(transform);
            draggedIcon.GetComponent<RectTransform>().position = _rect.position;

            // 슬롯에 이미 다른 아이콘이 있으면 제거
            if (transform.childCount > 1)
                for (int i = 0; i < transform.childCount - 1; i++)
                    Destroy(transform.GetChild(0).gameObject);
        }
    }

    // 마우스가 슬롯 위에 올라올 때
    public void OnPointerEnter(PointerEventData eventData)
    {
        _image.color = Color.yellow; // 슬롯의 색상을 노란색으로 변경
    }

    // 마우스가 슬롯에서 벗어날 때
    public void OnPointerExit(PointerEventData eventData)
    {
        _image.color = Color.white; // 원래 색상으로 복귀
    }
}
