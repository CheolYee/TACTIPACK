using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MapManager : MonoBehaviour
{
    public enum MapType
    {
        Enemy, Shop, Reward, Rest
    }

    public static MapManager Instance { get; private set; }

    [Header("Chapter Objects")]
    [SerializeField] private GameObject chapter1Obj;
    [SerializeField] private GameObject chapter2Obj;

    private GameObject currentChapterObj; // 지금 화면에 보이는 챕터

    [SerializeField] private Transform chapter1Root;
    [SerializeField] private Transform chapter2Root;
    private Transform currentRoot;

    [Header("맵 복귀 위치")]
    [SerializeField] private Transform mapPosition;

    [Header("랜덤 맵 카메라 위치")]
    [SerializeField] private Transform enemyCamPoint;
    [SerializeField] private Transform shopCamPoint;
    [SerializeField] private Transform rewardCamPoint;
    [SerializeField] private Transform restCamPoint;

    [Header("챕터1의 마지막 맵들 (전부 클리어 조건용)")]
    [SerializeField] private List<MapSO> chapter1ClearMaps = new();

    private readonly List<MapNode> _mapNodes = new();
    private MapSO currentMap;

    private bool chapter1Cleared = false; // 챕터1이 클리어된 지 아닌지 확인용

    private void Awake()
    {
        Instance = this;

        currentRoot = chapter1Root; //처음에는 챕터1 루트로 설정하고
    }

    private void Start()
    {
        chapter1Obj.SetActive(true);
        chapter2Obj.SetActive(false);
        currentChapterObj = chapter1Obj;

        CacheNodes();
    }

    private void Update()
    {
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            ClearMap(currentMap);
            Debug.Log($"{currentMap.mapName} 클리어!");
        }
    }

    private void CacheNodes()
    {
        _mapNodes.Clear();
        currentRoot.GetComponentsInChildren(true, _mapNodes);
    }

    public void OnMapClicked(MapSO clickedMap)
    {
        currentMap = clickedMap;
        currentChapterObj.SetActive(false);

        if (!clickedMap.isRandom)
        { CameraController.Instance.MoveToPoint(clickedMap.cameraPosition); return; }

        float rand = Random.value * 100f;
        Transform targetPoint;

        if (rand < 85f) targetPoint = enemyCamPoint;
        else if (rand < 90f) targetPoint = shopCamPoint;
        else if (rand < 95f) targetPoint = rewardCamPoint;
        else targetPoint = restCamPoint;

        CameraController.Instance.MoveToPoint(targetPoint);
    }

    public void ClearMap(MapSO currentMap)
    {
        if (currentMap == null) return;

        foreach (var next in currentMap.nextMap)
            next.isLook = false;

        if (!chapter1Cleared)
            CheckChapter1ClearCondition();

        CameraController.Instance.MoveToPoint((Vector2)mapPosition.position);


        UpdateAllNodes();


        currentChapterObj.SetActive(true);
    }

    private void CheckChapter1ClearCondition()
    {
        foreach (var so in chapter1ClearMaps)
        {
            if (so.isLook) return;
        }

        chapter1Cleared = true;

        chapter1Obj.SetActive(false);
        chapter2Obj.SetActive(true);
        currentChapterObj = chapter2Obj;

        currentRoot = chapter2Root;
        CacheNodes();
        UpdateAllNodes();
    }


    private void UpdateAllNodes()
    {
        foreach (var node in _mapNodes)
            node.UpdateVisual();
    }
}
