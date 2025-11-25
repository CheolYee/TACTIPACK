using System.Collections;
using System.Collections.Generic;
using _00.Work.Resource.Scripts.Managers;
using _00.Work.Scripts.Managers;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Events;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Managers;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Save;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Stages.Maps;
using _00.Work.WorkSpace.CheolYee._04.Scripts.UI.Turn;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;  // ★ 메인 메뉴 로드용

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
        
        [Header("Chapter 2 Clear Nodes (엔딩 후보)")]
        [SerializeField] private List<MapSo> chapter2ClearMaps = new();
        
        [Header("Map Transition")]
        [SerializeField] private float mapSlideDuration = 0.4f;
        [SerializeField] private Vector2 mapShownPos = Vector2.zero;   // 맵이 화면에 있을 때 위치
        [SerializeField] private Vector2 mapHiddenPos = new(0, -800f); // 화면 아래로 내려간 위치

        [Header("Starting Chapter")]
        [SerializeField] private int startingChapter = 1;

        [Header("Chapter Backgrounds")] // ★ 챕터별 배경
        [SerializeField] private GameObject chapter1Background;
        [SerializeField] private GameObject chapter2Background;

        [Header("Game Clear")] //전체 클리어용
        [SerializeField] private GameObject gameClearPanel;
        [SerializeField] private int mainMenuSceneIndex;
        
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
            
            SoundManager.Instance.PlayBgm(BgmId.Map);
            
            var saveMgr = SaveManager.Instance;
            MapProgressSaveData save = null;

            if (saveMgr != null)
            {
                save = saveMgr.LoadMapProgress();
            }

            if (save is { nodes: { Count: > 0 } })
            {
                //저장된 맵 진행도 적용
                ApplyMapProgress(save);
            }
            else
            {
                //세이브가 없으면 기본 초기 상태 생성
                Initialize();
            }
        }

        // 현재 챕터에 맞게 배경/루트 활성화 갱신 ★
        private void UpdateChapterVisuals()
        {
            // 배경
            if (chapter1Background != null)
                chapter1Background.SetActive(_currentChapter == 1);

            if (chapter2Background != null)
                chapter2Background.SetActive(_currentChapter == 2);

            // 맵 루트
            if (chapter1Root != null)
                chapter1Root.gameObject.SetActive(_currentChapter == 1);

            if (chapter2Root != null)
                chapter2Root.gameObject.SetActive(_currentChapter == 2);
        }

        // 현재 챕터의 루트 반환 ★
        private RectTransform GetCurrentChapterRoot()
        {
            if (_currentChapter == 1) return chapter1Root;
            if (_currentChapter == 2) return chapter2Root;
            return chapter1Root != null ? chapter1Root : chapter2Root;
        }
        
        //한줄딸깍 저장메서드
        public void SaveCurrentMapProgress()
        {
            var saveMgr = SaveManager.Instance;
            if (saveMgr == null) return;

            var data = BuildMapProgressSaveData();
            saveMgr.SaveMapProgress(data);
        }
        
        //저장된 맵 데이터를 기반으로 맵 설정하기
        private MapProgressSaveData BuildMapProgressSaveData()
        {
            var data = new MapProgressSaveData
            {
                currentChapter = _currentChapter,
                lastClearedMapId = _lastClearedMap != null ? _lastClearedMap.mapId : -1
            };

            foreach (var kv in _nodeStates)
            {
                MapSo map = kv.Key;
                MapNodeState state = kv.Value;
                if (map == null) continue;

                data.nodes.Add(new MapNodeSaveEntry
                {
                    mapId = map.mapId,
                    state = state
                });
            }

            return data;
        }

        private void ApplyMapProgress(MapProgressSaveData data)
        {
            if (data == null)
            {
                Debug.LogWarning("[MapManager] 적용할 맵 세이브 데이터가 없습니다.");
                return;
            }

            //챕터 / 현재맵 정보 세팅
            _currentChapter = data.currentChapter > 0 ? data.currentChapter : startingChapter;
            _currentMapInStage = null; // 로드시에는 항상 맵 UI 기준에서 시작하게
            _lastClearedMap = FindMapById(data.lastClearedMapId);

            //모든 노드를 일단 Locked로 초기화
            _nodeStates.Clear();
            foreach (var kv in _nodeViews)
            {
                MapSo map = kv.Key;
                if (map == null) continue;
                _nodeStates[map] = MapNodeState.Locked;
            }

            //세이브에 기록된 상태들 반영
            if (data.nodes != null)
            {
                foreach (var entry in data.nodes)
                {
                    var map = FindMapById(entry.mapId);
                    if (map == null) continue;

                    _nodeStates[map] = entry.state;
                }
            }

            RefreshAllNodes();
            UpdateChapterVisuals(); // ★ 로드시 챕터 비주얼 갱신
        }
        
        private MapSo FindMapById(int mapId)
        {
            if (mapId < 0) return null;

            foreach (var kv in _nodeViews)
            {
                MapSo map = kv.Key;
                if (map != null && map.mapId == mapId)
                    return map;
            }

            return null;
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

        private void Initialize()
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
            UpdateChapterVisuals(); // ★ 초기화 시에도 비주얼 갱신
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

            var battle = BattleSkillManager.Instance;
            if (battle != null)
            {
                battle.ClearAllStatusEffectsForAllAgents();
            }
            
            _currentMapInStage = map;

            StartCoroutine(EnterStageRoutine(map));
        }

        private IEnumerator EnterStageRoutine(MapSo map)
        {
            RectTransform root = GetCurrentChapterRoot(); // ★ 현재 챕터 루트 사용
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

            CheckChapterClear(map); // ★ 챕터 클리어/게임 클리어 체크
            RefreshAllNodes();
        }
        
        public void ShowCurrentChapterRoot(bool animated = true)
        {
            TurnUiContainerPanel.Instance.IsTurnRunning = true;
            CacheRootPositions();   // 혹시 아직 안 했으면 한 번 캐싱
            
            SoundManager.Instance.PlayBgm(BgmId.Map);

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

            UpdateChapterVisuals(); // ★ 루트/배경 동기화

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

        // 1챕터 클리어 시 2챕터 오픈 ★
        private void OpenChapter2()
        {
            _currentChapter = 2;

            // 2챕터 시작 노드 열어주기 (이미 Cleared인 노드는 그대로 유지)
            foreach (var start in chapter2StartMaps)
            {
                if (start == null) continue;

                if (_nodeStates.TryGetValue(start, out var state) &&
                    state == MapNodeState.Cleared)
                    continue;

                _nodeStates[start] = MapNodeState.Available;
            }

            UpdateChapterVisuals();
        }

        // 전체 게임 클리어 처리 ★
        private void HandleGameClear()
        {
            Debug.Log("[MapManager] 게임 클리어!");

            // 1) 세이브 전부 삭제
            var saveMgr = SaveManager.Instance;
            if (saveMgr != null)
            {
                saveMgr.DeleteAllSaves();
            }

            // 2) 맵 UI 끄기
            if (chapter1Root != null) chapter1Root.gameObject.SetActive(false);
            if (chapter2Root != null) chapter2Root.gameObject.SetActive(false);

            // 3) 클리어 패널 표시
            if (gameClearPanel != null)
            {
                gameClearPanel.SetActive(true);
            }
            else
            {
                // 패널이 없으면 바로 메인 메뉴로
                OnGameClearConfirm();
            }
        }

        // 클리어 패널의 버튼에서 호출하면 됨 ★
        public void OnGameClearConfirm()
        {
            var fade = FadeManager.Instance;
            if (fade != null)
            {
                fade.FadeToSceneAsync(mainMenuSceneIndex);
            }
            else
            {
                SceneManager.LoadScene(mainMenuSceneIndex);
            }
        }

        //챕터가 전부 클리어 되었는지 검사
        private void CheckChapterClear(MapSo map)
        {
            if (map == null)
            {
                Debug.LogWarning("[MapManager] CheckChapterClear 호출됐지만 map 이 null 입니다.");
                return;
            }

            Debug.Log($"[MapManager] CheckChapterClear : currentChapter={_currentChapter}, clearedMap={map.mapName} (id={map.mapId})");

            // 1챕터 엔딩 노드 클리어 → 2챕터 오픈
            if (_currentChapter == 1)
            {
                // 리스트 안에 있는지 확인 로그
                foreach (var clearMap in chapter1ClearMaps)
                {
                    if (clearMap == null) continue;
                    Debug.Log($"[MapManager] chapter1ClearMaps contains: {clearMap.mapName} (id={clearMap.mapId})");
                }

                if (chapter1ClearMaps.Contains(map))
                {
                    Debug.Log("[MapManager] Chapter 1 clear detected. Opening Chapter 2.");
                    OpenChapter2();
                    return;
                }
            }

            // 2챕터 엔딩 노드 클리어 → 게임 클리어
            if (_currentChapter == 2)
            {
                foreach (var clearMap in chapter2ClearMaps)
                {
                    if (clearMap == null) continue;
                    Debug.Log($"[MapManager] chapter2ClearMaps contains: {clearMap.mapName} (id={clearMap.mapId})");
                }

                if (chapter2ClearMaps.Contains(map))
                {
                    Debug.Log("[MapManager] Chapter 2 clear detected. Game Clear!");
                    HandleGameClear();
                }
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
