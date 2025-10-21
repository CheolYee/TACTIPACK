using System.Collections.Generic;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Managers;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.UI
{
    public class GridInventoryUIController : MonoBehaviour
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
        
        private GridItemGhostUI _ghost; //런타임 인스턴스

        // 드래그 상태
        private ItemInstance _dragItem; // 드래그 중인 아이템 인스턴스
        private ItemDataSo _dragData; // 드래그하는 아이템의 SO
        private int _dragRotation; //드래그 중 회전(0/90/180/270)
        private Vector2Int _lastAnchor = new(int.MinValue, int.MinValue); //유령 위치 캐시

        // 데이터 캐시
        private readonly Dictionary<string, ItemDataSo> _dataCache = new();
        private readonly List<Vector2Int> _abs = new();
        private readonly List<int> _idxs = new();

        private void Start()
        {
            _ghost = Instantiate(ghostPrefab, grid.gridRect); //그리드 패널 아래에 생성
            _ghost.Hide(); //시작 시 다 숨김
            
            database.Initialize(); //내부 딕셔너리 구성
            foreach (ItemDataSo so in database.allItems) //등록된 so 순회
            {
                _dataCache[so.itemId] = so; //빠른 조회를 위해 캐싱
            }
            
            if (uiCamera == null) uiCamera = canvas != null ? canvas.worldCamera : Camera.main;
            
            if (gridSlots != null)
            {
                gridSlots.BuildSlots(); //슬롯 생성
                gridSlots.RefreshColors(); //색 초기화
            }
        }

        [ContextMenu("Pick RandomItem")] //랜덤 아이템을 잡아와요
        public void PickRandomItem()
        {
            if (database.allItems == null || database.allItems.Count == 0) return; //데이터 없으면 리턴
            ItemDataSo so = database.allItems[Random.Range(0, database.allItems.Count)]; //랜덤 so 하나 고름
            
            StartDrag(new ItemInstance(so.itemId), so, 0);
        }

        private void StartDrag(ItemInstance itemInstance, ItemDataSo data, int rotation)
        {
            _dragItem = itemInstance; //드래드 인스턴스 설정
            _dragData = data; //데이터 설정
            _dragRotation = rotation; //회전값 설정
            _ghost.gameObject.SetActive(true); //유령 표시
            _lastAnchor = new Vector2Int(int.MinValue, int.MinValue); //강제 갱신 유도
            
            if (hover != null) hover.Hide(); //드래그 중일 때는 호버 없애기
        }

        //드래그 끝
        private void StopDrag()
        {
            _dragItem = null; //해제
            _dragData = null; //해제
            _ghost.Hide(); //숨김
        }

        private void Update()
        {
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
                    var cell = grid.ScreenToCell(uiCamera, Input.mousePosition, out var oob);
                    if (!oob) hover.ShowAt(cell); else hover.Hide();
                }
            }
        }

        private void HandleInput()
        {
            //회전
            if (_dragItem != null && Keyboard.current.rKey.wasPressedThisFrame)
            {
                _dragRotation = (_dragRotation + 90) % 360; //90도 회전
                _lastAnchor = new Vector2Int(int.MinValue, int.MinValue); //유령 재배치 유도
            }

            Vector2Int cell = grid.ScreenToCell(uiCamera, Input.mousePosition, out bool outOfBounds); //셀 좌표 추출

            if (_dragItem != null) //드래그 아이템이 있다면
            {
                if (outOfBounds)
                {
                    _ghost.Hide();
                    if (Input.GetMouseButtonDown(0)) { /* 무시 */ }
                    if (Input.GetMouseButtonDown(1))  { StopDrag(); } // 우클릭 취소는 허용
                    return;
                }
                
                UpdateGhost(cell, true); //유령 위치 & 색 갱신

                if (Input.GetMouseButtonDown(0))
                {
                    TryPlace(cell, true); //셀 안이라면 설치 시도
                }

                if (Input.GetMouseButtonUp(1))
                {
                    StopDrag(); //우클릭 시 드래그 종료
                }
                
                return; //드래그 처리 끝
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
                    grid.Remove(picked);

                    //슬롯 배경 색 갱신(있다면)
                    if (gridSlots != null) gridSlots.RefreshColors();

                    //드래그 시작(기존 회전 유지)
                    StartDrag(picked, so, picked.rotation);
                }
            }
        }

        private void UpdateGhost(Vector2Int anchorCell, bool inside)
        {
            if (!inside)
            {
                _ghost.Hide();
                return;
            }
            
            //앵커가 바뀌지 않았다면 스킵
            if (anchorCell == _lastAnchor) return; //동일 위치면 패스
            _lastAnchor = anchorCell; //캐시 갱신
            
            bool ok = grid.CanPlace(_dragItem, _dragData, anchorCell, _dragRotation); //가능 여부 검사
            _ghost.SetOk(ok);
            
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
    }
}
