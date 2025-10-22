using UnityEngine;
using UnityEngine.UI;

public class MapNode : MonoBehaviour
{
    [SerializeField] private Button mapButton;
    [SerializeField] private MapSO mapData;

    private void Start()
    {
        mapButton.onClick.AddListener(OnClickMap);
        UpdateVisual();
    }

    public void UpdateVisual()
    {
        mapButton.interactable = !mapData.isLook;  // 만약 isLook가 true면 애도 true, false면 애도 false
    }

    private void OnClickMap()
    {
        if (mapData.isLook) return; // 잠겨있으면 클릭 못하게 만들기

        MapManager.Instance.OnMapClicked(mapData);
    }
}
