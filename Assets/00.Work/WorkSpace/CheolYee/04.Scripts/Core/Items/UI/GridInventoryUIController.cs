using System;
using System.Collections.Generic;
using _00.Work.Scripts.Managers;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Events;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.UI.SideItem;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.UI.SideItem.SIdeInventoryItem;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Managers;
using _00.Work.WorkSpace.CheolYee._04.Scripts.UI.Turn;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.UI
{
    public class GridInventoryUIController : MonoSingleton<GridInventoryUIController>
    {
        [Header("Refs")]
        public GridInventoryUIManager grid; //그리드 로직/UI 좌표 변환
        public ItemDatabase database; //아이템 DB(SO)
        public Canvas canvas; //이 UI가 속한 Canvas
        public Camera uiCamera; //Canvas Render Camera

        [Header("Ghost")]
        public GridItemGhostUI ghostPrefab; //유령 프리팹(UGUI Image들)

        [Header("Visual Layers")] 
        public GridVisualSlotsUI gridSlots; //슬롯 배경
        public PlaceItemLayerUI placedLayer; //배치된 아이템 렌더
        public HoverCellUI hover; //호버 하이라이트
        
        [Header("Drag SideInventory")]
        [SerializeField] private SideInventoryManager sideManager;
        [SerializeField] private Image dragIconPrefab;
        
        private Image _dragIcon;
        
        private GridItemGhostUI _ghost; //런타임 인스턴스
        private DragOrigin _dragOrigin = DragOrigin.None;
        private SideInventoryManager _sideManagerForRefund; //사이드 출처면 환불 대상
        private bool _pendingRefund; //설치 실패/취소 시 환불할지

        // 드래그 상태
        private ItemInstance _dragItem; //드래그 중인 아이템 인스턴스
        private ItemDataSo _dragData; //드래그하는 아이템의 SO
        private int _dragRotation; //드래그 중 회전(0/90/180/270)
        private int _originRotation; //회전값 저장
        private Vector2Int _lastAnchor = new(int.MinValue, int.MinValue); //유령용
        
        
        //바인딩 모드
        private bool _bindingMode;
        private Action<ItemInstance> _onBindingItemSelected;

        // 데이터 캐시
        private readonly Dictionary<string, ItemDataSo> _dataCache = new();
        private readonly List<Vector2Int> _abs = new();
        private readonly List<int> _idxs = new();

        #region BindingMode

        //바인딩 시작
        public void EnterBindingMode(Action<ItemInstance> onItemSelected)
        {
            _bindingMode = true;
            _onBindingItemSelected = onItemSelected;
        }

        public void ExitBindingMode()
        {
            _bindingMode = false;
            _onBindingItemSelected = null;
        }


        #endregion

        private void Start()
        {
            if (_ghost == null) _ghost = Instantiate(ghostPrefab, grid.gridRect); //그리드 패널 아래에 생성
            _ghost.Hide(); //시작 시 다 숨김
            
            database.Initialize(); //내부 딕셔너리 구성
            foreach (ItemDataSo so in database.allItems.ItemDatabase) //등록된 so 순회
            {
                _dataCache[so.itemId] = so; //빠른 조회를 위해 캐싱
            }
            
            if (uiCamera == null) uiCamera = canvas != null ? canvas.worldCamera : Camera.main;
            
            if (gridSlots != null)
            {
                gridSlots.BuildSlots(); //슬롯 생성
                gridSlots.RefreshColors(); //색 초기화
            }
            
            //드래그 아이콘 준비
            if (dragIconPrefab != null && _dragIcon == null)
            {
                _dragIcon = Instantiate(dragIconPrefab, canvas.transform);
                _dragIcon.gameObject.SetActive(false);
            }
        }

        private void StartDrag(ItemInstance itemInstance, ItemDataSo data, int rotation
            , DragOrigin dragOrigin, SideInventoryManager sideManagerOrNull)
        {
            _dragItem = itemInstance; //드래드 인스턴스 설정
            _dragData = data; //데이터 설정
            _dragRotation = rotation; //회전값 설정
            
            _dragOrigin = dragOrigin;
            _sideManagerForRefund = sideManagerOrNull;
            
            _ghost.gameObject.SetActive(true); //유령 표시
            _lastAnchor = new Vector2Int(int.MinValue, int.MinValue); //강제 갱신 유도
            
            if (hover != null) hover.Hide(); //드래그 중일 때는 호버 없애기

            if (_dragOrigin == DragOrigin.Grid && _dragItem != null && grid != null)
            {
                grid.SetCooldownVisualVisible(_dragItem, false);
            }
            
            if (_dragIcon != null)
            {
                _dragIcon.sprite = _dragData != null ? _dragData.icon : null;
                _dragIcon.gameObject.SetActive(false);
            }
        }
        
        //사이드 슬롯에서 시작하는 진입점
        public void StartDragFromSide(ItemDataSo so, SideInventoryManager sideInventoryManager)
        {
            // 새 인스턴스 생성(설치 성공 시 그리드가 소유)
            var inst = new ItemInstance(so.itemId);
            
            if (so != null && so.isConsumable)
            {
                inst.InitUses(so.maxUses);
            }
            
            StartDrag(inst, so, 0, DragOrigin.Side, sideInventoryManager);
            _pendingRefund = true;
        }

        //드래그 끝
        private void StopDrag()
        {
            if (_pendingRefund && _dragOrigin == DragOrigin.Side && _sideManagerForRefund != null && _dragData != null)
            {
                _sideManagerForRefund.AddItem(_dragData);
            }

            _pendingRefund = false;
            _sideManagerForRefund = null;
            _dragOrigin = DragOrigin.None;

            _dragItem = null;
            _dragData = null;

            _ghost.Hide();
            if (_dragIcon != null) _dragIcon.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (IsBlockedByTurn())
            {
                //드래그 중이면 강제로 취소
                if (_dragItem != null)
                {
                    StopDrag();
                }

                //호버도 숨기기
                if (hover != null)
                {
                    hover.Hide();
                }
                
                if (_ghost != null)
                    _ghost.Hide();

                return; //입력 처리 전부 스킵
            }
            
            HandleInput();

            //호버 업데이트
            if (hover != null)
            {
                if (_dragItem != null)
                {
                    //드래그(설치)중에는 혼동 방지를 위해 호버 숨김
                    hover.Hide();
                }
                else
                {
                    // 드래그 중이 아닐 때만 호버 표시
                    Vector2Int cell = grid.ScreenToCell(uiCamera, Input.mousePosition, out bool oob);
                    if (!oob) hover.ShowAt(cell); else hover.Hide();
                }
            }
        }

        private void HandleInput()
        {
            Vector2Int cell = grid.ScreenToCell(uiCamera, Input.mousePosition, out bool outOfBounds); //셀 좌표 추출

            //바인딩 모드
            if (_bindingMode)
            {
                if (!outOfBounds && Input.GetMouseButtonDown(0))
                {
                    ItemInstance picked = grid.GetItemAtCell(cell);
                    _onBindingItemSelected?.Invoke(picked);
                }
                
                //바인드에선 기존 회전과 설치로직을 막음
                return;
            }
            
            //회전
            if (_dragItem != null && Keyboard.current.rKey.wasPressedThisFrame)
            {
                _dragRotation = (_dragRotation + 90) % 360; //90도 회전
                _lastAnchor = new Vector2Int(int.MinValue, int.MinValue); //유령 재배치 유도
            }


            if (_dragItem != null) // 드래그 중
            {
                if (outOfBounds)
                {
                    _ghost.Hide();
                    UpdateDragIconFollow(true);

                    if (Input.GetMouseButtonUp(0))
                    {
                        if (_dragOrigin == DragOrigin.Side)
                        {
                            //사이드에서 끌고 나온 아이템은 그냥 드래그 취소
                            StopDrag();
                        }
                        else if (_dragOrigin == DragOrigin.Grid)
                        {
                            if (_dragItem.IsOnCooldown)
                            {
                                Bus<MessageEvent>.Raise(new MessageEvent("쿨타임중인 아이템은 돌아갈 수 없습니다."));
                                return;
                            }

                            if (_dragData != null && _dragData.isConsumable)
                            {
                                int maxUses = Mathf.Max(1, _dragData.maxUses);
                                if (_dragItem.HasLimitedUses && _dragItem.RemainingUses < maxUses)
                                {
                                    Bus<MessageEvent>.Raise(new MessageEvent("사용한 소모성 아이템은 되돌릴 수 없습니다."));
                                    return;
                                }
                            }
                            
                            //쿨타임이 아닌 아이템은 기존처럼 사이드 인벤토리로 돌아감
                            if (sideManager != null && _dragData != null)
                            {
                                if (grid != null)
                                    grid.Remove(_dragItem);

                                Bus<OnItemReturnedToSideInventory>.Raise(
                                    new OnItemReturnedToSideInventory(_dragItem));

                                sideManager.AddItem(_dragData);
                            }

                            StopDrag();
                        }
                    }

                    return;
                }

                // 안쪽은 기존 그대로 (UpdateGhost / TryPlace)
                UpdateDragIconFollow(false);
                UpdateGhost(cell, true);

                if (Input.GetMouseButtonUp(0))
                {
                    TryPlace(cell, true);
                }
                return;
            }
            
            //드래그 중이 아닐 때
            if (!outOfBounds && Input.GetMouseButtonDown(0)) //그리드 범위 내 좌클릭
            {
                ItemInstance picked = grid.GetItemAtCell(cell); //클릭한 셀의 아이템
                if (picked != null) //뭔가 있으면
                {
                    ItemDataSo so = _dataCache[picked.dataId]; //SO 조회

                    //시각화 제거(배치 타일)
                    if (placedLayer != null) placedLayer.HideItem(picked);

                    //그리드에서 떼기
                    grid.DetachForDrag(picked);

                    //슬롯 배경 색 갱신(있다면)
                    if (gridSlots != null) gridSlots.RefreshColors();

                    //드래그 시작(기존 회전 유지)
                    StartDrag(picked, so, picked.rotation, DragOrigin.Grid, null);
                }
            }
        }
        
        private void UpdateDragIconFollow(bool show)
        {
            if (_dragIcon == null) return;
            if (!show)
            {
                if (_dragIcon.gameObject.activeSelf) _dragIcon.gameObject.SetActive(false);
                return;
            }

            if (!_dragIcon.gameObject.activeSelf) _dragIcon.gameObject.SetActive(true);

            //화면 좌표 그대로 따라가기
            RectTransform t = (RectTransform)_dragIcon.transform;
            Vector2 screenPos = Input.mousePosition;
            
            t.position = screenPos; 
        }

        private void UpdateGhost(Vector2Int anchorCell, bool inside)
        {
            if (!inside)
            {
                _ghost.Hide();
                return;
            }
            
            bool ok = grid.CanPlace(_dragItem, _dragData, anchorCell, _dragRotation); //가능 여부 검사
            _ghost.SetOk(ok);
            
            if (ok && anchorCell != _lastAnchor)
            {
                _lastAnchor = anchorCell;
            }
            
            //유령 타일들의 고정 위치 목록 만들기
            List<Vector2Int> absCells = grid.GetAbsoluteCells(_dragItem, _dragData, anchorCell, _dragRotation);
            List<Vector2> anchors = new List<Vector2>(absCells.Count);
            foreach (Vector2Int cell in absCells)
            {
                Vector2 ap = grid.CellToAnchoredPos(cell);
                anchors.Add(ap);
            }
            _ghost.SetCellsAnchored(anchors);
            _ghost.gameObject.SetActive(true);
        }

        private void TryPlace(Vector2Int anchorCell, bool inside)
        {
            if (!inside)
            {
                Debug.Log("그리드 영역 밖입니다.");
                return;
            }

            //만약 이동중인 인스턴스라면 자기가 사용하던 칸은 다시 쓸 수 있으므로 검사를 함
            if (!grid.HasCapacityFor(_dragData, _dragItem))
            {
                int need = grid.GetRequiredCellCount(_dragData); //필요 칸 수
                int free = grid.CountFreeCells(); //현재 빈 칸 수
                Debug.LogWarning($"빈 칸 수가 부족합니다 : 필요한 칸 수 {need}칸, 현재 빈 칸 {free}칸");
                return; //중단
            }

            //배치가 가능한가?
            if (grid.CanPlace(_dragItem, _dragData, anchorCell, _dragRotation))
            {
                //설치가 성공하면 환불 금지
                _pendingRefund = false;
                
                //배치한다
                grid.Place(_dragItem, _dragData, anchorCell, _dragRotation);
                
                if (placedLayer != null) //배치된 아이템 타일 렌더
                {
                    //절대셀과 로컬 인덱스 쌍 생성
                    _abs.Clear();
                    _idxs.Clear();
                    grid.GetAbsoluteCellsWithIndex(_dragItem, _dragData, anchorCell, _dragRotation, _abs, _idxs);
                    
                    //셀별 스프라이트 그리기
                    placedLayer.ShowItem(_dragItem, _dragData, _abs, _idxs, _dragRotation);
                }
                if (gridSlots != null) gridSlots.RefreshColors(); //칸 색 갱신
                
                StopDrag(); //고스트 숨기기
            }
            else
            {
                Debug.LogWarning("해당 위치에 배치할 수 없습니다.");
            }
        }
        
        private bool IsBlockedByTurn()
        {
            var panel = TurnUiContainerPanel.Instance;
            return panel != null && panel.IsTurnRunning;
        }
    }
}
