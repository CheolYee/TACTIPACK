using System.Collections.Generic;
using _00.Work.Resource.Scripts.Utils;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Events;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.ItemTypes.PassiveItems;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.UI;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Save;
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
        
        [Header("Slot Visual")]
        [SerializeField] private GridVisualSlotsUI slotVisual;
        
        [Header("Consumable Visual")]
        [SerializeField] private RectTransform consumableTextPrefab;   //소모성 전용 프리팹
        [SerializeField] private Vector2 consumableMargin = new(20, 20); //위치 보정
        
        [Header("Data")]
        [SerializeField] private ItemDatabase itemDatabase;
        
        //각 셀이 어떤 아이템 인스턴스에 의해 사용되었는지 저장(없으면 null이 담김)
        private ItemInstance[,] _cells; //그리드형 인벤토리를 위한 아이템 인스턴스 참조를 위해 2차원 배열 생성

        //각 배치된 아이템의 인스턴스 아이디를 사용되어지고 있는 셀 목록(절대 좌표)를 캐싱
        private readonly Dictionary<string, List<Vector2Int>> _occupiedMap = new();
        
        private readonly Dictionary<ItemInstance, Vector2Int> _anchorByInstance = new();
        //인스턴스별 쿨타임 텍스트
        private readonly Dictionary<ItemInstance, RectTransform> _cooldownVisuals = new();
        
        //쿨타임 턴 수 제작
        private readonly Dictionary<ItemInstance, int> _cooldownMaxTurns = new();
        
        //소모성
        private readonly Dictionary<ItemInstance, RectTransform> _consumableVisuals = new();
        private readonly Dictionary<ItemInstance, int> _consumableMaxUses = new();
        
        // 각 인스턴스가 어떤 앵커 셀에 배치됐는지 기억
        
        //재사용을 위한 임시 버퍼
        private readonly List<Vector2Int> _tmpRotated = new();
        private readonly List<int> _tmpIndices = new();

        
        #region Save
        
        public GridInventorySaveData CaptureSaveData()
        {
            var save = new GridInventorySaveData
            {
                width = width,
                height = height
            };

            if (itemDatabase == null)
            {
                itemDatabase = FindFirstObjectByType<ItemDatabase>();
            }

            // 그리드에서 유니크한 인스턴스들 뽑기
            EnsureGrid();
            int w = _cells.GetLength(0);
            int h = _cells.GetLength(1);

            var visited = new HashSet<ItemInstance>();

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    var inst = _cells[x, y];
                    if (inst == null || !visited.Add(inst))
                        continue;

                    // 인스턴스의 아이템 데이터 찾기
                    ItemDataSo data = null;
                    if (itemDatabase != null)
                    {
                        data = itemDatabase.GetItemById(inst.dataId);
                    }

                    if (data == null)
                    {
                        Debug.LogWarning($"[GridInventory] 저장 중: ItemData 를 찾지 못했습니다. dataId={inst.dataId}");
                        continue;
                    }

                    // anchor 추출
                    var anchorOpt = TryFindAnchorForInstance(inst, data);
                    if (!anchorOpt.HasValue)
                    {
                        Debug.LogWarning($"[GridInventory] anchor 계산 실패, inst={inst.instanceId}, dataId={inst.dataId}");
                        continue;
                    }

                    Vector2Int anchor = anchorOpt.Value;

                    var entry = new GridItemSaveEntry
                    {
                        dataId = inst.dataId,
                        anchorX = anchor.x,
                        anchorY = anchor.y,
                        rotation = inst.rotation,
                        remainingCooldown = inst.RemainingCooldownTurns,
                        remainingUses = inst.HasLimitedUses ? inst.RemainingUses : -1
                    };

                    save.items.Add(entry);
                }
            }

            return save;
        }
        
        public void ApplySaveData(GridInventorySaveData save)
        {
            // 기존 모든 아이템 제거
            WipeAllItems();

            if (save == null || save.items == null || save.items.Count == 0)
                return;

            if (itemDatabase == null)
            {
                itemDatabase = FindFirstObjectByType<ItemDatabase>();
            }
            
            var cooldownMgr = ItemCooldownManager.Instance;

            foreach (var entry in save.items)
            {
                if (string.IsNullOrEmpty(entry.dataId))
                    continue;

                ItemDataSo data = itemDatabase != null
                    ? itemDatabase.GetItemById(entry.dataId)
                    : null;

                if (data == null)
                {
                    Debug.LogWarning($"[GridInventory] 로드 중: ItemData 를 찾지 못했습니다. dataId={entry.dataId}");
                    continue;
                }

                // 새 인스턴스 생성
                var inst = new ItemInstance(entry.dataId)
                {
                    rotation = entry.rotation
                };

                // 소모성 사용 횟수 복원
                if (data.isConsumable)
                {
                    // 기본 uses 세팅
                    inst.InitUses(data.maxUses);

                    if (entry.remainingUses >= 0)
                    {
                        inst.ForceSetUsesForLoad(entry.remainingUses);
                    }
                }

                // 그리드에 배치
                Vector2Int anchor = new Vector2Int(entry.anchorX, entry.anchorY);

                if (!CanPlace(inst, data, anchor, inst.rotation))
                {
                    Debug.LogWarning($"[GridInventory] 저장된 위치에 배치할 수 없습니다. dataId={entry.dataId}, anchor=({anchor.x},{anchor.y})");
                    continue;
                }

                //설치
                Place(inst, data, anchor, inst.rotation);
                
                //비주얼 복원
                if (placeLayer != null)
                {
                    var absCells = new List<Vector2Int>();
                    var indices  = new List<int>();
                    GetAbsoluteCellsWithIndex(inst, data, anchor, inst.rotation, absCells, indices);
                    placeLayer.ShowItem(inst, data, absCells, indices, inst.rotation);
                }

                // 쿨타임 복원 (배치 후에 하는 것이 중요)
                if (entry.remainingCooldown > 0)
                {
                    inst.StartCooldown(entry.remainingCooldown);
                    cooldownMgr?.RegisterFromLoad(inst);
                }
            }

            if (slotVisual != null)
                slotVisual.RefreshColors();
        }
        
        private void WipeAllItems()
        {
            EnsureGrid();
            int w = _cells.GetLength(0);
            int h = _cells.GetLength(1);

            var visited = new HashSet<ItemInstance>();

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    var inst = _cells[x, y];
                    if (inst == null || !visited.Add(inst))
                        continue;

                    Remove(inst); // 기존 Remove 로 패시브/비주얼까지 정리
                }
            }

            // 내부 배열 초기화
            _occupiedMap.Clear();
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    _cells[x, y] = null;
                }
            }

            placeLayer?.ClearAll();
            
            if (slotVisual != null)
                slotVisual.RefreshColors();
        }
        
        public void ApplyGridInventorySave(GridInventorySaveData data)
        {
            if (data == null)
            {
                Debug.LogWarning("[GridInventoryUIManager] 적용할 그리드 세이브 데이터가 없습니다.");
                return;
            }

            EnsureGrid();

            if (itemDatabase == null)
            {
                itemDatabase = FindFirstObjectByType<ItemDatabase>();
            }

            if (itemDatabase == null)
            {
                Debug.LogError("[GridInventoryUIManager] ItemDatabase가 없어 인벤토리 로드를 할 수 없습니다.");
                return;
            }

            // 현재 그리드가 비어있다는 가정(씬 시작 시 호출용).
            // 혹시 남아있다면 필요에 따라 전체 초기화 함수 따로 만들어서 호출해도 됨.
            // _cells, _occupiedMap, _anchorByInstance, placeLayer.ClearAll() 등을 정리.

            foreach (var entry in data.items)
            {
                var so = itemDatabase.GetItemById(entry.dataId);
                if (so == null)
                {
                    Debug.LogWarning($"[GridInventoryUIManager] 저장된 아이템 {entry.dataId} 를 찾을 수 없어 스킵합니다.");
                    continue;
                }

                var inst = new ItemInstance(entry.dataId);

                // 소모성 사용 횟수 복원
                if (entry.remainingUses >= 0)
                {
                    inst.InitUses(entry.remainingUses);
                }

                // 쿨타임 복원 (이 안에서 이벤트도 날아감)
                if (entry.remainingCooldown > 0)
                {
                    inst.StartCooldown(entry.remainingCooldown);
                }

                int rotation = entry.rotation;
                Vector2Int anchor = new Vector2Int(entry.anchorX, entry.anchorY);

                // 혹시 그리드가 바뀌어서 설치가 불가능한 경우 방어
                if (!CanPlace(inst, so, anchor, rotation))
                {
                    Debug.LogWarning($"[GridInventoryUIManager] 저장 위치 {anchor}에 {entry.dataId}를 둘 수 없어 스킵합니다.");
                    continue;
                }

                Place(inst, so, anchor, rotation);
            }
        }
        
        private Vector2Int? TryFindAnchorForInstance(ItemInstance inst, ItemDataSo data)
        {
            if (inst == null || data == null) return null;
            if (!_occupiedMap.TryGetValue(inst.instanceId, out var cells) || cells == null || cells.Count == 0)
                return null;

            // 현재 이 인스턴스가 차지하고 있는 셀 집합
            var cellSet = new HashSet<Vector2Int>(cells);

            // 로컬 오프셋 및 회전된 오프셋
            var baseOffsets = data.GetShapeOffsets();
            if (baseOffsets == null || baseOffsets.Length == 0)
                return null;

            var rotatedOffsets = GridShapeUtil.GetRotatedOffsets(baseOffsets, inst.rotation, data.pivot);
            if (rotatedOffsets == null || rotatedOffsets.Length == 0)
                return null;

            // 가능한 모든 (absCell, rotatedOffset) 조합에 대해 anchor 후보를 찾아보고
            // 그 anchor 로 다시 돌렸을 때 셀 집합이 정확히 같은지 검사
            foreach (var abs in cells)
            {
                foreach (var ro in rotatedOffsets)
                {
                    // abs = anchor + ro - pivot  =>  anchor = abs - ro + pivot
                    Vector2Int candidateAnchor = abs - ro + data.pivot;

                    bool ok = true;
                    foreach (var off in rotatedOffsets)
                    {
                        var c = candidateAnchor + off - data.pivot;
                        if (!cellSet.Contains(c))
                        {
                            ok = false;
                            break;
                        }
                    }

                    if (ok)
                    {
                        return candidateAnchor;
                    }
                }
            }

            return null;
        }
        
        #endregion
        
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
            
            //쿨타임과 소모성 다지우기
            foreach (var kvp in _cooldownVisuals)
            {
                if (kvp.Value != null)
                    Destroy(kvp.Value.gameObject);
            }
            _cooldownVisuals.Clear();
            
            foreach (var kvp in _consumableVisuals)
            {
                if (kvp.Value != null)
                    Destroy(kvp.Value.gameObject);
            }
            _consumableVisuals.Clear();
            _consumableMaxUses.Clear();
        }


        private void Start()
        {
            EnsureGrid();
        }

        #region Consumable
        
        private Vector2 GetConsumableVisualPosition(Vector2Int cell, int rotation)
        {
            // 셀 중심
            Vector2 center = CellToAnchoredPos(cell);

            // 좌측 하단 기준 오프셋
            Vector2 baseOffset = new Vector2(cellSizePx * 0.5f, -cellSizePx * 0.5f) + consumableMargin;
    
            // 회전 보정
            Vector2 rotatedOffset = Util.RotateOffset(baseOffset, rotation);
    
            return center + rotatedOffset;
        }
        
        
        //그리드에 배치될 떄 UI 생성과 갱신
        private void RefreshConsumableVisual(
            ItemInstance inst,
            ItemDataSo data,
            List<Vector2Int> absCells)
        {
            if (consumableTextPrefab == null) return;

            // 소모성이 아니거나, 사용 제한이 없거나, 이미 소진되면 UI 제거
            if (inst == null || data == null || !data.isConsumable || !inst.HasLimitedUses || inst.IsDepleted)
            {
                if (inst != null && _consumableVisuals.TryGetValue(inst, out var old) && old != null)
                {
                    Destroy(old.gameObject);
                }

                if (inst != null)
                {
                    _consumableVisuals.Remove(inst);
                    _consumableMaxUses.Remove(inst);
                }

                return;
            }

            if (absCells == null || absCells.Count == 0)
                return;

            // 첫 번째 셀을 기준으로 앵커 위치 계산
            Vector2Int anchorCell = absCells[0];
            Vector2 anchoredPos = GetConsumableVisualPosition(anchorCell, inst.rotation);

            RectTransform visual;
            if (_consumableVisuals.TryGetValue(inst, out var existing) && existing != null)
            {
                visual = existing;
                visual.anchoredPosition = anchoredPos;
            }
            else
            {
                visual = Instantiate(consumableTextPrefab, gridRect);
                visual.anchoredPosition = anchoredPos;
                _consumableVisuals[inst] = visual;
            }

            int maxUses = Mathf.Max(1, data.maxUses);
            _consumableMaxUses[inst] = maxUses;

            var tmp = visual.GetComponentInChildren<TextMeshProUGUI>(true);
            if (tmp != null)
            {
                tmp.text = inst.RemainingUses.ToString();
            }

            visual.gameObject.SetActive(true);
        }
        
        
        //사용한 텍스트 소진처리
        public void UpdateConsumableUsesVisual(ItemInstance inst)
        {
            if (inst == null) return;

            if (!_consumableVisuals.TryGetValue(inst, out var visual) || visual == null)
                return;

            var tmp = visual.GetComponentInChildren<TextMeshProUGUI>(true);
            if (tmp != null)
            {
                tmp.text = inst.RemainingUses.ToString();
            }

            // 전부 소진되면 UI 제거
            if (inst.IsDepleted)
            {
                Destroy(visual.gameObject);
                _consumableVisuals.Remove(inst);
                _consumableMaxUses.Remove(inst);
            }
        }

        #endregion

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
            //쿨타임과 소모성 Ui 둘다 끄고키기
            if (inst == null) return;
            if (_cooldownVisuals.TryGetValue(inst, out var cdVisual) && cdVisual != null)
            {
                cdVisual.gameObject.SetActive(visible);
            }

            if (_consumableVisuals.TryGetValue(inst, out var consVisual) && consVisual != null)
            {
                consVisual.gameObject.SetActive(visible);
            }
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
            return cell is { x: >= 0, y: >= 0 } && cell.x < w && cell.y < h;
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
            
            _anchorByInstance[inst] = anchor; //앵커 기억
            
            if (_cooldownVisuals.TryGetValue(inst, out var label) && label != null)
            {
                Vector2Int anchorCell = absCells[0];
                Vector2 anchoredPos = GetCooldownVisualPosition(anchorCell, inst.rotation);
                label.anchoredPosition = anchoredPos;
                label.gameObject.SetActive(true);
            }
            
            RefreshConsumableVisual(inst, data, absCells);
            
            //So가 패시브라면
            if (data is PassiveItemSo passive)
            {
                Bus<PassiveItemEquippedEvent>.Raise(new PassiveItemEquippedEvent(passive, inst));
            }
        }

        public void Remove(ItemInstance inst)
        {
            ItemDataSo data = null;
            if (inst != null)
            {
                if (itemDatabase == null)
                {
                    itemDatabase = FindFirstObjectByType<ItemDatabase>();
                }

                if (itemDatabase != null)
                {
                    data = itemDatabase.GetItemById(inst.dataId);
                }
            }
            
            ClearInstanceCells(inst, false);
            placeLayer?.HideItem(inst);
            placeLayer?.ClearItemTint(inst);
            if (inst != null)
            {
                _cooldownMaxTurns.Remove(inst);

                if (data is PassiveItemSo passive)
                {
                    Bus<PassiveItemUnequippedEvent>.Raise(new PassiveItemUnequippedEvent(passive, inst));
                }
            }
        }
        
        public void DetachForDrag(ItemInstance inst)
        {
            ClearInstanceCells(inst, keepCooldownVisual: true);
        }
        
        private void ClearInstanceCells(ItemInstance inst, bool keepCooldownVisual)
        {
            if (inst == null) return;
    
            //셀 비우기
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
            
            //앵커도 정리
            _anchorByInstance.Remove(inst);

            // 쿨타임 비주얼까지 지울지 여부
            if (!keepCooldownVisual)
            {
                if (_cooldownVisuals.TryGetValue(inst, out var label) && label != null)
                {
                    Destroy(label.gameObject);
                }
                _cooldownVisuals.Remove(inst);
                
                if (_consumableVisuals.TryGetValue(inst, out var cLabel) && cLabel != null)
                    Destroy(cLabel.gameObject);
                _consumableVisuals.Remove(inst);
                _consumableMaxUses.Remove(inst);
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