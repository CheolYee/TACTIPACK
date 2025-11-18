using System;
using System.Collections.Generic;
using _00.Work.Scripts.Managers;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Events;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Stages.Maps;
using UnityEngine;

namespace _00.Work.WorkSpace.JaeHun._01._Scrpits
{
    public class MapManager : MonoSingleton<MapManager>
    {
        [Header("Chapter Roots (UI)")]
        [SerializeField] private Transform chapter1Root;
        [SerializeField] private Transform chapter2Root;

        [Header("Chapter Start Nodes")]
        [SerializeField] private List<MapSo> chapter1StartMaps = new();
        [SerializeField] private List<MapSo> chapter2StartMaps = new();

        [Header("Chapter 1 Clear Nodes (엔딩 후보)")]
        [SerializeField] private List<MapSo> chapter1ClearMaps = new();

        [Header("Starting Chapter")]
        [SerializeField] private int startingChapter = 1;
        
        private readonly Dictionary<MapSo, MapNode> _nodeViews = new();
        private readonly Dictionary<MapSo, MapNodeState> _nodeStates = new();

        private int _currentChapter;
        private MapSo _currentMapInStage;   //지금 전투/이벤트 중인 맵
        private MapSo _lastClearedMap; 
        private void Start()
        {
            CacheNodeViews();
            Initialize();
        }

        private void CacheNodeViews()
        {
            _nodeViews.Clear();

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
            if (!CanEnter(map)) return;
            
            _currentMapInStage = map;
            
            chapter1Root.gameObject.SetActive(false);

            //스테이지 메니저가 이거 받아서 처리하기
            Bus<MapEnteredEvent>.Raise(new MapEnteredEvent(map));
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
