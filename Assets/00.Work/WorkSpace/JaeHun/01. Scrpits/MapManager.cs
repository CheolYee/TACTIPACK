using System.Collections;
using System.Collections.Generic;
using _00.Work.Resource.Scripts.Managers;
using _00.Work.Scripts.Managers;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Events;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Stages.Maps;
using _00.Work.WorkSpace.CheolYee._04.Scripts.UI.Turn;
using DG.Tweening;
using UnityEngine;

namespace _00.Work.WorkSpace.JaeHun._01._Scrpits
{
    public class MapManager : MonoSingleton<MapManager>
    {
        [Header("Chapter Roots (UI)")]
        [SerializeField] private RectTransform chapter1Root;
        [SerializeField] private RectTransform chapter2Root;

        [Header("Chapter Start Nodes")]
        [SerializeField] private List<MapSo> chapter1StartMaps = new();
        [SerializeField] private List<MapSo> chapter2StartMaps = new();

        [Header("Chapter 1 Clear Nodes (엔딩 후보)")]
        [SerializeField] private List<MapSo> chapter1ClearMaps = new();
        
        [Header("Map Transition")]
        [SerializeField] private float mapSlideDuration = 0.4f;
        [SerializeField] private Vector2 mapShownPos = Vector2.zero;   // 맵이 화면에 있을 때 위치
        [SerializeField] private Vector2 mapHiddenPos = new(0, -800f); // 화면 아래로 내려간 위치

        [Header("Starting Chapter")]
        [SerializeField] private int startingChapter = 1;
        
        private Vector2 _chapter1ShownPos;
        private Vector2 _chapter2ShownPos;
        private bool _rootPosCached;
        
        private readonly Dictionary<MapSo, MapNode> _nodeViews = new();
        private readonly Dictionary<MapSo, MapNodeState> _nodeStates = new();

        private int _currentChapter;
        private MapSo _currentMapInStage;   //지금 전투/이벤트 중인 맵
        private MapSo _lastClearedMap; 
        public MapSo CurrentMapInStage => _currentMapInStage;
        private void Start()
        {
            CacheNodeViews();
            Initialize();
        }

        private void CacheNodeViews()
        {
            _nodeViews.Clear();

            CacheRootPositions();
            
            if (chapter1Root != null)
            {
                MapNode[] nodes = chapter1Root.GetComponentsInChildren<MapNode>(true);
                foreach (var node in nodes)
                {
                    RegisterNode(node);
                }
            }

            if (chapter2Root != null)
            {
                MapNode[] nodes = chapter2Root.GetComponentsInChildren<MapNode>(true);
                foreach (var node in nodes)
                {
                    RegisterNode(node);
                }
            }
        }
        
        private void CacheRootPositions()
        {
            if (_rootPosCached) return;

            if (chapter1Root != null)
                _chapter1ShownPos = chapter1Root.anchoredPosition;

            if (chapter2Root != null)
                _chapter2ShownPos = chapter2Root.anchoredPosition;

            _rootPosCached = true;
        }

        private void RegisterNode(MapNode node)
        {
            MapSo mapSo = node.MapData;
            if (mapSo == null) return;
            
            _nodeViews.TryAdd(mapSo, node);
        }

        public void Initialize()
        {
            _nodeStates.Clear();

            //모든 노드는 락 상태로 시작한다
            foreach (var kvp in _nodeViews)
            {
                _nodeStates[kvp.Key] = MapNodeState.Locked;
            }
            
            _currentChapter = startingChapter;
            _currentMapInStage = null;
            _lastClearedMap = null;

            //시작 시 챕터에 따라 노드를 모두 활성
            if (_currentChapter == 1)
            {
                foreach (var start in chapter1StartMaps)
                {
                    if (start == null) continue;
                    _nodeStates[start] = MapNodeState.Available;
                }
            }
            else if (_currentChapter == 2)
            {
                foreach (var start in chapter2StartMaps)
                {
                    if (start == null) continue;
                    _nodeStates[start] = MapNodeState.Available;
                }
            }

            RefreshAllNodes();
        }

        public bool CanEnter(MapSo map)
        {
            return GetNodeState(map) == MapNodeState.Available;
        }

        public MapNodeState GetNodeState(MapSo map)
        {
            if (map == null) return MapNodeState.Locked;
            return _nodeStates.GetValueOrDefault(map, MapNodeState.Locked);
        }

        //UI에서 클릭 시 호출
        public void OnMapClickedFromUi(MapSo map)
        {
            if (!CanEnter(map) || map == null) return;

            _currentMapInStage = map;

            StartCoroutine(EnterStageRoutine(map));
        }

        private IEnumerator EnterStageRoutine(MapSo map)
        {
            RectTransform root = chapter1Root;
            if (root == null) yield break;

            root.gameObject.SetActive(true);

            // 1) 맵을 아래로 슬라이드
            root.anchoredPosition = mapShownPos;
            var slideDown = root
                .DOAnchorPos(mapHiddenPos, mapSlideDuration)
                .SetEase(Ease.InSine);

            yield return slideDown.WaitForCompletion();

            if (FadeManager.Instance != null)
            {
                
                FadeManager.Instance.FadeIn(() =>
                {
                    root.gameObject.SetActive(false);
                    Bus<MapEnteredEvent>.Raise(new MapEnteredEvent(map));
                    FadeManager.Instance.FadeOut();
                });
            }
        }

        public void CompleteCurrentMap()
        {
            if (_currentMapInStage == null) return;
            
            MapSo map = _currentMapInStage;
            _currentMapInStage = null;
            _lastClearedMap = map;
            
            _nodeStates[map] = MapNodeState.Cleared;
            
            var keys = new List<MapSo>(_nodeStates.Keys);
            foreach (MapSo key in keys)
            {
                if (_nodeStates[key] == MapNodeState.Available)
                    _nodeStates[key] = MapNodeState.Locked;
            }

            if (map.nextMap != null)
            {
                foreach (MapSo next in map.nextMap)
                {
                    if (next == null) continue;
                    
                    if (_nodeStates.TryGetValue(next, out var state) &&
                        state == MapNodeState.Cleared)
                        continue;
                    
                    _nodeStates[next] = MapNodeState.Available;
                }
            }

            CheckChapterClear(map);
            RefreshAllNodes();
        }
        
        public void ShowCurrentChapterRoot(bool animated = true)
        {
            TurnUiContainerPanel.Instance.IsTurnRunning = true;
            CacheRootPositions();   // 혹시 아직 안 했으면 한 번 캐싱

            RectTransform rt = null;
            Vector2 shownPos = Vector2.zero;

            if (_currentChapter == 1 && chapter1Root != null)
            {
                rt = chapter1Root;
                shownPos = _chapter1ShownPos;
            }
            else if (_currentChapter == 2 && chapter2Root != null)
            {
                rt = chapter2Root;
                shownPos = _chapter2ShownPos;
            }

            if (rt == null) return;

            var go = rt.gameObject;
            go.SetActive(true);

            if (!animated)
            {
                rt.anchoredPosition = shownPos;
                return;
            }

            // 항상 아래에서 시작해서, 캐싱해 둔 "보이는 위치"로 올라오게
            rt.anchoredPosition = new Vector2(shownPos.x, -Screen.height);

            rt.DOAnchorPos(shownPos, 0.5f)
                .SetEase(Ease.OutCubic);
        }

        

        //챕터가 전부 클리어 되었는지 검사
        private void CheckChapterClear(MapSo map)
        {
            if (_currentChapter == 1 && chapter1ClearMaps.Contains(map))
            {
                //TODO:2챕터 오픈
            }
        }

        private void RefreshAllNodes()
        {
            foreach (var pair in _nodeViews)
            {
                pair.Value.RefreshVisual();
            }
        }
    }
}
