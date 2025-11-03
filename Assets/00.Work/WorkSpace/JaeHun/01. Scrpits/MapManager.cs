using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    [SerializeField] private GameObject mapUI;
    [SerializeField] private Transform nodeRoot;        // MapNode들이 들어있는 Content Transform
    [SerializeField] private Transform mapPosition;

    private readonly List<MapNode> _mapNodes = new();
    private MapSO currentMap;

    private void Awake()
    {
        Instance = this;
        nodeRoot.GetComponentsInChildren(includeInactive: true, _mapNodes);
    }

    private void Update()
    {
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            ClearMap(currentMap);
            Debug.Log($"[MapManager] {currentMap.mapName} 클리어!");
        }
    }

    public void OnMapClicked(MapSO clickedMap)
    {
        CameraController.Instance.MoveToPoint(clickedMap.cameraPosition);  // 해당 맵 카메라 위치로 이동
        currentMap = clickedMap; // 현재맵은 내가 클릭한 맵.
        ShowMap(false);
        Debug.Log($"[MapManager] 현재 맵: {currentMap.mapName}");
    }

    public void ClearMap(MapSO currentMap)
    {
        foreach (var next in currentMap.nextMap)
        {
            next.isLook = false;
        }

        UpdateAllNodes();
        ShowMap(true);
        CameraController.Instance.MoveToPoint((Vector2)mapPosition.position);
        //CameraController.Instance.MoveToPoint(
    }

    private void UpdateAllNodes()
    {
        foreach (var node in _mapNodes)
        {
            node.UpdateVisual();
        }
    }

    public void ShowMap(bool value) // 성태한테 솔리드 뭐시기를 배우고 나니 뭔가 코드를 이렇게 써야 멋질거 같아서 이렇게 씀.
    {
        mapUI.SetActive(value);
    }
}
