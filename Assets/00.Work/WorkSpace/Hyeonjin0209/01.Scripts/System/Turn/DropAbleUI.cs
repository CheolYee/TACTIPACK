using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class DropAbleUI : MonoBehaviour, IPointerEnterHandler, IDropHandler , IPointerExitHandler
{   
    private Image _image;
    private RectTransform _rect;

    private void Awake()
    {
         _image = GetComponent<Image>();
        _rect = GetComponent<RectTransform>();

    }
    public void OnDrop(PointerEventData eventData)
    {
        if(eventData.pointerDrag != null)
        {
            DragAbleUI draggedIcon = eventData.pointerDrag.GetComponent<DragAbleUI>();

            DragAbleUI copy =
                Instantiate(draggedIcon.gameObject, draggedIcon.Canvas).GetComponent<DragAbleUI>();

            copy.gameObject.name = draggedIcon.gameObject.name;

            copy.previousParent = draggedIcon.previousParent;
            copy.OnEndDrag(eventData);

            draggedIcon.transform.SetParent(transform);
            draggedIcon.GetComponent<RectTransform>().position = _rect.position;
            if (transform.childCount > 1)
                for (int i = 0; i < transform.childCount - 1; i++)
                    Destroy(transform.GetChild(0).gameObject);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _image.color = Color.yellow;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _image.color = Color.white;
    }
}
