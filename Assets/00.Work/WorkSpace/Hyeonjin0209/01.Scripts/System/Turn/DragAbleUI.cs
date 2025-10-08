using UnityEngine;
using UnityEngine.EventSystems;

public class TurnOrderDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{

    private Transform _canvas;
    private Transform _previousParent;
    private RectTransform _rect;
    private CanvasGroup _canvasGroup;


    private void Awake()
    {
        _canvas = FindFirstObjectByType<Canvas>().transform;
        _rect = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _previousParent = transform.parent;

        transform.SetParent(_canvas);
        transform.SetAsLastSibling();

        _canvasGroup.alpha = 0.5f;  
        _canvasGroup.blocksRaycasts = false;
    }
    public void OnDrag(PointerEventData eventData)
    {
        _rect.position = eventData.position;
    }
    public void OnEndDrag(PointerEventData eventData)
    {
       if(transform.parent == _canvas)
        {
            transform.SetParent(_previousParent);
            _rect.position = _previousParent.GetComponent<RectTransform>().position;
        }
        _canvasGroup.alpha = 1f;
        _canvasGroup.blocksRaycasts = true;
    }
}
