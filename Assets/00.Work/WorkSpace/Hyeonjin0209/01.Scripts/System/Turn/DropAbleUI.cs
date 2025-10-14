using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class DropAbleUI : MonoBehaviour, IPointerEnterHandler, IDropHandler , IPointerExitHandler
{   
    private Image _image;
    private RectTransform _rect;

    public GameObject[] icons;
    private Transform[] startPoint;

    private void Awake()
    {
         _image = GetComponent<Image>();
        _rect = GetComponent<RectTransform>();
    }

    private void Start()
    {
        startPoint = new Transform[icons.Length];
        for (int i = 0; i < icons.Length; i++)
        {
            startPoint[i] = icons[i].transform;
        }

    }

    public void OnDrop(PointerEventData eventData)
    {
        if(eventData.pointerDrag != null)
        {
            GameObject draggedIcon = eventData.pointerDrag;

            eventData.pointerDrag.transform.SetParent(transform);
            eventData.pointerDrag.GetComponent<RectTransform>().position = _rect.position;
            for (int i = 0; i < icons.Length; i++)
            {
                if(draggedIcon.name == icons[i].name)
                {
                    Instantiate(icons[i], startPoint[i].transform.position, Quaternion.identity);
                }
            }
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
