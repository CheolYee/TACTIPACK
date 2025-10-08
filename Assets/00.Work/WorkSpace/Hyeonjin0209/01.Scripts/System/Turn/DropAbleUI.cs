using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class DropAbleUI : MonoBehaviour, IPointerEnterHandler, IDropHandler , IPointerExitHandler
{

    private void Awake()
    {
         _image = GetComponent<Image>();
        _rect = GetComponent<RectTransform>();
    }
    private Image _image;
    private RectTransform _rect;

    public void OnDrop(PointerEventData eventData)
    {
        if(eventData.pointerDrag != null)
        {
            eventData.pointerDrag.transform.SetParent(transform);
            eventData.pointerDrag.GetComponent<RectTransform>().position = _rect.position;
          
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
