using System.Collections.Generic;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Managers
{
    public class GridInventoryManager : MonoBehaviour
    {
        [Header("Grid Size")]
        public int width = 10; //그리드 가로 칸 수
        public int height = 6; //그리드 세로 칸 수

        [Header("UI")] 
        public RectTransform gridRect; //백팩 그리드가 표시될 위치
        public float cellSizePx = 64f; //셀 한 칸 크기
        public Vector2 cellPaddingPx = Vector2.zero; //셀 한칸 간격
        public Vector2Int pivotAsCell; //피벗을 셀 좌표 기준으로 볼 때의 기준(기본 0, 0)
        
        //각 셀이 어떤 아이템 인스턴스에 의해 사용되었는지 저장(없으면 null이 담김)
        private ItemInstance[,] _cells; //그리드형 인벤토리를 위한 아이템 인스턴스 참조를 위해 2차원 배열 생성

        //각 배치된 아이템의 인스턴스 아이디를 사용되어지고 있는 셀 목록(절대 좌표)를 캐싱
        private readonly Dictionary<string, List<Vector2Int>> _occupiedMap = new();

        private void Awake()
        {
            _cells = new ItemInstance[width, height]; //모든 셀을 null로 초기화
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
            //먼저 스크린을 gridRect 로컬 좌표로 변환
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                gridRect, screenPos, uiCamera, out Vector2 local); // local은 pivot 원점 기준

            //좌하단 기준으로 보정
            Vector2 grid = LocalToGridSpace(local); // 좌하단 기준 좌표

            //셀+패딩 크기
            float stepX = cellSizePx + cellPaddingPx.x; // X축 한 칸 간격
            float stepY = cellSizePx + cellPaddingPx.y; // Y축 한 칸 간격

            //셀 인덱스 계산(바닥 내림)
            int cellX = Mathf.FloorToInt(grid.x / stepX); // x셀
            int cellY = Mathf.FloorToInt(grid.y / stepY); // y셀

            //경계 검사
            outOfBounds = (cellX < 0 || cellY < 0 || cellX >= width || cellY >= height);
            return new Vector2Int(cellX, cellY); // 결과 반환
        }

        //셀 인덱스를 gridRect 로컬 좌표(좌하단 원점)로 변환
        public Vector2 CellToAnchoredPos(Vector2Int cell)
        {
            //셀+패딩 크기
            float stepX = cellSizePx + cellPaddingPx.x; //X축 한 칸 간격
            float stepY = cellSizePx + cellPaddingPx.y; //Y축 한 칸 간격

            //좌하단 기준 로컬 좌표
            Vector2 gridLocal = new Vector2(cell.x * stepX, cell.y * stepY); //좌하단 기준 위치

            //pivot 보정(anchoredPosition은 pivot 원점 기준)
            Vector2 size = gridRect.rect.size; //그리드 전체 사이즈
            Vector2 local = gridLocal - size * gridRect.pivot;  //pivot 원점으로 환산

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
                Vector2Int cell = anchor + offset; //앵커 기준 절대 셀
                abs.Add(cell); //결과에 추가
            }
            return abs; //절대 좌표 리스트 반환
        }

        private bool InBounds(Vector2Int cell)
        {
            //왼쪽 아래 시작점이 0임 0부터 가로 세로 최대까지 되는지 확인함
            return cell is { x: >= 0, y: >= 0 } && cell.x < width && cell.y < height; 
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
            Remove(inst); //기존 설치칸 모두 제거
            
            List<Vector2Int> absCells = GetAbsoluteCells(inst, data, anchor, rotation); //설치 셀 목록

            foreach (Vector2Int cell in absCells) //각 셀들을
            {
                _cells[cell.x, cell.y] = inst; //이 인스턴스로 채운다
            }
            
            _occupiedMap[inst.instanceId] = absCells; //인스턴스가 설치되는 셀 목록 계산
            
            inst.isInBackpack = true; //백팩에 있으니까 true변경
            inst.backpackPosition = anchor; //기준 위치 기록
            inst.rotation = rotation; //회전 상태 기록
        }

        public void Remove(ItemInstance inst)
        {
            if (inst == null) return; //예외처리
            if (_occupiedMap.TryGetValue(inst.instanceId, out List<Vector2Int> cells)) //캐시에 셀 목록이 있다면
            {
                foreach (Vector2Int cell in cells) //각 셀을 검사
                {
                    if (InBounds(cell) && _cells[cell.x, cell.y] == inst) //여전히 같은 인스턴스면
                    {
                        _cells[cell.x, cell.y] = null; //설치 해제
                    }
                }
                _occupiedMap.Remove(inst.instanceId); //캐시에서 제거
            }
            
            inst.isInBackpack = false; //상태 갱신
        }

        public bool Move(ItemInstance inst, ItemDataSo data, Vector2Int anchor, int rotation)
        {
            if (!CanPlace(inst, data, anchor, rotation)) //설치가 불가능하면
                return false; //실패
            Place(inst, data, anchor, rotation); //가능했다면 설치
            return true; //성공
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
    }
}