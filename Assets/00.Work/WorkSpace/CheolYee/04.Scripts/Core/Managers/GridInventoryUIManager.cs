using System.Collections.Generic;
using _00.Work.Resource.Scripts.Utils;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Events;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.UI;
using TMPro;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Managers
{
    public class GridInventoryUIManager : MonoBehaviour
    {
        [Header("Grid Size")]
        public int width = 10; //그리드 가로 칸 수
        public int height = 6; //그리드 세로 칸 수

        [Header("UI")] 
        public RectTransform gridRect; //백팩 그리드가 표시될 위치
        public float cellSizePx = 64f; //셀 한 칸 크기
        public Vector2 cellPaddingPx = Vector2.zero; //셀 한칸 간격
        
        [Header("CoolDown Visual")]
        [SerializeField] private RectTransform cooldownTextPrefab;
        [SerializeField] private Vector2 cooldownMargin = new(-20, 20);
        
        [Header("Render Layers")]
        [SerializeField] private PlaceItemLayerUI placeLayer;
        
        //각 셀이 어떤 아이템 인스턴스에 의해 사용되었는지 저장(없으면 null이 담김)
        private ItemInstance[,] _cells; //그리드형 인벤토리를 위한 아이템 인스턴스 참조를 위해 2차원 배열 생성

        //각 배치된 아이템의 인스턴스 아이디를 사용되어지고 있는 셀 목록(절대 좌표)를 캐싱
        private readonly Dictionary<string, List<Vector2Int>> _occupiedMap = new();
        
        //인스턴스별 쿨타임 텍스트
        private readonly Dictionary<ItemInstance, RectTransform> _cooldownVisuals = new();
        
        //쿨타임 턴 수 제작
        private readonly Dictionary<ItemInstance, int> _cooldownMaxTurns = new();
        
        
        //재사용을 위한 임시 버퍼
        private readonly List<Vector2Int> _tmpRotated = new();
        private readonly List<int> _tmpIndices = new();

        
        private void Awake()
        {
            if (width < 1) width = 1;
            if (height < 1) height = 1;
            EnsureGrid();
            
            Bus<ItemCooldownChangedEvent>.OnEvent += OnItemCooldownChanged;
            Bus<ItemCooldownStartedEvent>.OnEvent += OnItemCooldownStarted;
        }


        private void OnDestroy()
        {
            Bus<ItemCooldownChangedEvent>.OnEvent -= OnItemCooldownChanged;
            Bus<ItemCooldownStartedEvent>.OnEvent -= OnItemCooldownStarted;
            
            foreach (var kvp in _cooldownVisuals)
            {
                if (kvp.Value != null)
                    Destroy(kvp.Value.gameObject);
            }
            _cooldownVisuals.Clear();
        }


        private void Start()
        {
            EnsureGrid();
        }

        #region CoolTimes
        
        private void OnItemCooldownStarted(ItemCooldownStartedEvent evt)
        {
            ItemInstance inst = evt.Instance;
            if (inst == null) return;
            if (evt.CooldownTurns <= 0) return;

            _cooldownMaxTurns[inst] = evt.CooldownTurns;
            placeLayer?.SetItemCooldownTint(inst, 1f);

            // 시작할 때는 ratio = 1 (가장 어두운 상태)
            if (placeLayer != null)
            {
                placeLayer.SetItemCooldownTint(inst, 1f);
            }
        }

        public void SetCooldownVisualVisible(ItemInstance inst, bool visible)
        {
            if (inst == null) return;
            if (!_cooldownVisuals.TryGetValue(inst, out var visual) || visual == null) return;

            visual.gameObject.SetActive(visible);
        }

        private Vector2 GetCooldownVisualPosition(Vector2Int cell, int rotation)
        {
            // 현재 CellToAnchoredPos는 '셀 중심' 기준 좌표를 반환하고 있음
            Vector2 center = CellToAnchoredPos(cell);
            Vector2 baseOffset = new Vector2(-cellSizePx * 0.5f, -cellSizePx * 0.5f) + cooldownMargin;
            
            Vector2 rotatedOffset = Util.RotateOffset(baseOffset, rotation);

            //좌측 하단 오프샛 맞추기
            return center + rotatedOffset;
        }
        private void OnItemCooldownChanged(ItemCooldownChangedEvent evt)
        {
            ItemInstance inst = evt.Instance;
            if (inst == null) return;

            //그리드에 이 인스턴스가 깔려 있는지 확인
            if (!_occupiedMap.TryGetValue(inst.instanceId, out var cells) || cells == null || cells.Count == 0)
            {
                //그리드에 없는데 텍스트가 있다면 제거
                if (_cooldownVisuals.TryGetValue(inst, out var visual) && visual != null)
                {
                    Destroy(visual.gameObject);
                }
                _cooldownVisuals.Remove(inst);
                
                _cooldownMaxTurns.Remove(inst);
                if (placeLayer != null)
                {
                    // 혹시 남아있을 렌더에 정상 색상으로 리셋
                    placeLayer.SetItemCooldownTint(inst, 0f);
                }
                return;
            }

            //남은 쿨타임이 0 이하라면 텍스트 제거
            if (evt.RemainingTurns <= 0)
            {
                if (_cooldownVisuals.TryGetValue(inst, out var visual) && visual != null)
                {
                    Destroy(visual.gameObject);
                }
                _cooldownVisuals.Remove(inst);
                
                _cooldownMaxTurns.Remove(inst);
                placeLayer?.SetItemCooldownTint(inst, 0f);
                return;
            }

            if (cooldownTextPrefab == null) return;

            //첫 번째 셀 기준
            Vector2Int anchorCell = cells[0];
            Vector2 anchoredPos = GetCooldownVisualPosition(anchorCell, inst.rotation);

            if (_cooldownVisuals.TryGetValue(inst, out var existing) && existing != null)
            {
                //위치 갱신
                existing.anchoredPosition = anchoredPos;

                //자식 TMP 찾아서 텍스트만 업데이트
                var tmp = existing.GetComponentInChildren<TextMeshProUGUI>(true);
                if (tmp != null)
                {
                    tmp.text = evt.RemainingTurns.ToString();
                }
            }
            else
            {
                //새 프리팹 생성
                RectTransform newVisual = Instantiate(cooldownTextPrefab, gridRect);
                newVisual.anchoredPosition = anchoredPos;

                var tmp = newVisual.GetComponentInChildren<TextMeshProUGUI>(true);
                if (tmp != null)
                {
                    tmp.text = evt.RemainingTurns.ToString();
                }

                _cooldownVisuals[inst] = newVisual;
            }
            
            if (placeLayer != null && _cooldownMaxTurns.TryGetValue(inst, out int maxTurns) && maxTurns > 0)
            {
                float ratio = (float)evt.RemainingTurns / maxTurns;   // 1 → 시작, 0 → 끝
                placeLayer.SetItemCooldownTint(inst, ratio);
            }
        }
        #endregion
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (width < 1) width = 1;
            if (height < 1) height = 1;
            //에디터에서 값 바꿀 때도 즉시 재빌드
            EnsureGrid();
        }
#endif

        //아이템이 차지할 셀 칸 수를 반환한다 (오프셋으로 만들어졌다면 오프셋 우선)
        public int GetRequiredCellCount(ItemDataSo data)
        {
            Vector2Int[] offset = data.GetShapeOffsets();
            return offset?.Length ?? (data.shape.x * data.shape.y);
        }

        //현재 그리드에서 완전히 빈 칸 수를 계산한다.
        public int CountFreeCells()
        {
            EnsureGrid();
            int w = _cells.GetLength(0);
            int h = _cells.GetLength(1);
            
            int free = 0;
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                if (_cells[x, y] == null) free++;

            return free;
        }

        //현재 빈 칸 수가 아이템 칸 수 이상인가?
        public bool HasCapacityFor(ItemDataSo data, ItemInstance inst = null)
        {
            EnsureGrid();

            int needed = GetRequiredCellCount(data);
            int free = CountFreeCells();

            // 이동 중인 아이템이면, 본인이 차지했던 칸 수만큼은 재사용 가능
            if (inst != null && _occupiedMap.TryGetValue(inst.instanceId, out var occ))
                free += occ.Count;

            return free >= needed;
        }

        //그리드 기준 로컬 좌표를 그리드 좌표계 좌측 하단 0,0으로 보정
        private Vector2 LocalToGridSpace(Vector2 localPos)
        {
            Vector2 size = gridRect.rect.size; //폭과 높이
            
            Vector2 bottomLeftOrigin = localPos + size * gridRect.pivot; //좌측 하단 원점(0,0)으로 보정
            return bottomLeftOrigin; //좌측 하단 기준 로컬 좌표 변환
        }
        
        //스크린 좌표를 셀 인덱스로 변환 (그리드 밖이면 outOfBounds = true)
        public Vector2Int ScreenToCell(Camera uiCamera, Vector2 screenPos, out bool outOfBounds)
        {
            //먼저 스크린좌표를 셀 인덱스 좌표로 변환
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                gridRect, screenPos, uiCamera, out Vector2 local); //local은 중심점 기준

            //좌하단 기준으로 보정
            Vector2 g = LocalToGridSpace(local); //좌하단 기준 좌표

            //셀+패딩 크기
            float stepX = cellSizePx + cellPaddingPx.x; //X축 한 칸 간격
            float stepY = cellSizePx + cellPaddingPx.y; //Y축 한 칸 간격

            //셀 인덱스 계산
            int cellX = Mathf.FloorToInt(g.x / stepX);
            int cellY = Mathf.FloorToInt(g.y / stepY);

            //경계 검사
            //셀 계산 방법: 0,0은 좌하단이며 그걸 기준으로 넘었는지 감지함
            outOfBounds = (cellX < 0 || cellY < 0 || cellX >= width || cellY >= height);
            
            return new Vector2Int(cellX, cellY); //결과 반환
        }

        //셀 인덱스를 gridRect 로컬 좌표(좌하단 원점)로 변환
        public Vector2 CellToAnchoredPos(Vector2Int cell)
        {
            //셀+패딩 크기
            float stepX = cellSizePx + cellPaddingPx.x; //X축 한 칸 간격
            float stepY = cellSizePx + cellPaddingPx.y; //Y축 한 칸 간격

            //좌하단 기준 로컬 좌표
            Vector2 gridLocal = new Vector2(cell.x * stepX, cell.y * stepY); //좌하단 기준 위치

            //오프셋이 셀 중앙 0.5, 0.5이므로 감지 범위가 명확하도록 조정
            Vector2 centerOffset = new Vector2(cellSizePx * 0.5f, cellSizePx * 0.5f);
            
            //pivot 보정(anchoredPosition은 pivot 원점 기준)
            Vector2 size = gridRect.rect.size; //그리드 전체 사이즈
            Vector2 local = (gridLocal + centerOffset) - size * gridRect.pivot;

            return local; //anchoredPosition에 쓸 좌표 반환
        }
        
        //특정 앵커와 회전 상태에서 이 아이템이 차지할 셀 크기를 계산
        public List<Vector2Int> GetAbsoluteCells(ItemInstance inst, ItemDataSo data, Vector2Int anchor, int rotation)
        {
            //아이템 기본 크기 (오프셋을 고려하여 크기 가져오기)
            Vector2Int[] baseOffsets = data.GetShapeOffsets(); //정의된 오프셋 목록 가져오기 (크기)
            
            //기준점 기준으로 회전
            Vector2Int[] rotateOffsets = GridShapeUtil.GetRotatedOffsets(baseOffsets, rotation, data.pivot);
            
            //절대 좌표 계산 (회전된 오프셋 - 기준점)
            List<Vector2Int> abs = new List<Vector2Int>(rotateOffsets.Length); //결과 리스트 미리 사이즈 맞게 할당
            //각 오프셋에 대하여
            foreach (Vector2Int offset in rotateOffsets)
            {
                //회전 결과는 기준점이 반영된 로컬 좌표라 앵커를 단순히 더하여 절대화 시킴
                Vector2Int cell = anchor + offset - data.pivot; //앵커 기준 절대 셀
                abs.Add(cell); //결과에 추가
            }
            return abs; //절대 좌표 리스트 반환
        }
        
        //인덱스 버전
        public void GetAbsoluteCellsWithIndex(
            ItemInstance inst, ItemDataSo data, Vector2Int anchor, int rotation,
            List<Vector2Int> outAbsCells, //결과 절대 셀 좌표들
            List<int> outLocalIndices) //각 절대 셀에 대응하는 오프셋 인덱스
        {
            outAbsCells.Clear();
            outLocalIndices.Clear();

            var baseOffsets = data.GetShapeOffsets(); //로컬 좌표 목록
            if (baseOffsets == null || baseOffsets.Length == 0) return;

            //회전 + 인덱스 보존
            _tmpRotated.Clear(); _tmpIndices.Clear();
            GridShapeUtil.GetRotatedOffsetsWithIndex(baseOffsets, rotation, data.pivot, _tmpRotated, _tmpIndices);

            //앵커가 중심칸이 되도록 보정(우리가 앞서 합의한 공식)
            for (int k = 0; k < _tmpRotated.Count; k++)
            {
                var abs = anchor + _tmpRotated[k] - data.pivot; //중요: -pivot 보정
                outAbsCells.Add(abs);
                outLocalIndices.Add(_tmpIndices[k]); //원본 인덱스 그대로
            }
        }

        //현재 그리드를 벗어나지 않았는가?
        private bool InBounds(Vector2Int cell)
        {
            EnsureGrid();
            int w = _cells.GetLength(0);
            int h = _cells.GetLength(1);
            return cell.x >= 0 && cell.y >= 0 && cell.x < w && cell.y < h;
        }

        //아이템을 설치할 수 있는가?
        public bool CanPlace(ItemInstance inst, ItemDataSo data, Vector2Int anchor, int rotation)
        {
            //절대 좌표 목록 산출
            List<Vector2Int> absCells = GetAbsoluteCells(inst, data, anchor, rotation);

            foreach (Vector2Int cell in absCells)
            {
                if (!InBounds(cell)) //그리드 밖을 벗어났다면?
                    return false; //설치 불가
                
                ItemInstance occupying = _cells[cell.x, cell.y]; //현재 셀을 차지한 인스턴스
                
                if (occupying != null && occupying != inst) //다른 인스턴스가 있다면?
                    return false; //설치 불가
            }
            return true; //모든 검사를 통과했다면 설치 가능
        }

        //실제 설치 (검사 통과되었다고 가정)
        public void Place(ItemInstance inst, ItemDataSo data, Vector2Int anchor, int rotation)
        {
            ClearInstanceCells(inst, true); //기존 설치칸 모두 제거
            
            List<Vector2Int> absCells = GetAbsoluteCells(inst, data, anchor, rotation); //설치 셀 목록

            foreach (Vector2Int cell in absCells) //각 셀들을
            {
                _cells[cell.x, cell.y] = inst; //이 인스턴스로 채운다
            }
            
            _occupiedMap[inst.instanceId] = absCells; //인스턴스가 설치되는 셀 목록 계산
            
            inst.rotation = rotation; //회전 상태 기록
            
            if (_cooldownVisuals.TryGetValue(inst, out var label) && label != null)
            {
                Vector2Int anchorCell = absCells[0];
                Vector2 anchoredPos = GetCooldownVisualPosition(anchorCell, inst.rotation);
                label.anchoredPosition = anchoredPos;
                label.gameObject.SetActive(true);
            }
        }

        public void Remove(ItemInstance inst)
        {
            ClearInstanceCells(inst, false);
            placeLayer?.ClearItemTint(inst);
            _cooldownMaxTurns.Remove(inst);
        }
        
        public void DetachForDrag(ItemInstance inst)
        {
            ClearInstanceCells(inst, keepCooldownVisual: true);
        }
        
        private void ClearInstanceCells(ItemInstance inst, bool keepCooldownVisual)
        {
            if (inst == null) return;
    
            // 셀 비우기 + _occupiedMap 정리
            if (_occupiedMap.TryGetValue(inst.instanceId, out List<Vector2Int> cells))
            {
                foreach (Vector2Int cell in cells)
                {
                    if (InBounds(cell) && _cells[cell.x, cell.y] == inst)
                    {
                        _cells[cell.x, cell.y] = null;
                    }
                }
                _occupiedMap.Remove(inst.instanceId);
            }

            // 쿨타임 비주얼까지 지울지 여부
            if (!keepCooldownVisual)
            {
                if (_cooldownVisuals.TryGetValue(inst, out var label) && label != null)
                {
                    Destroy(label.gameObject);
                }
                _cooldownVisuals.Remove(inst);
            }
        }

        //특정 셀이 차지되고있는가
        public bool IsOccupied(Vector2Int cell)
        {
            if (!InBounds(cell)) return true; //경계 밖은 true로 취급함
            return _cells[cell.x, cell.y] != null; //차지 여부 반환
        }

        public ItemInstance GetItemAtCell(Vector2Int cell)
        {
            if (!InBounds(cell)) return null; //경계 검사
            return _cells[cell.x, cell.y]; //차지된 칸 인스턴스 반환
        }

        //셀 크기를 현재 설정된 가로 세로와 동기화
        private void EnsureGrid()
        {
            //셀이 만들어지지 않았거나, 가로, 세로가 설정된것과 다르다면
            if (_cells == null || _cells.GetLength(0) != width || _cells.GetLength(1) != height)
            {
                //잘못된 셀이니 새로 생성한다.
                _cells = new ItemInstance[width, height];
                
                //기존 정보 초기화
                _occupiedMap.Clear();
            }
        }
    }
}