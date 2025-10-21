using System.Collections.Generic;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.UI
{
    public class PlaceItemLayerUI : MonoBehaviour
    {
        [Header("References")] 
        public GridInventoryUIManager grid; //좌표변환용
        public RectTransform parentTransform; //랜더 부모
        public Image tileTemplate; //한칸 타일
        public Sprite fallbackSprite; //프리뷰 없을 때 대체
        
        //키 : 인스턴스 아이디, 값: 이 아이템을 표현하는 타일 이미지
        private readonly Dictionary<string, List<Image>> _rendered = new();

        private void Awake()
        {
            if (tileTemplate != null) tileTemplate.gameObject.SetActive(false); //템플릿 꺼두기
        }

        //화면에 아이템을 그리기
        public void ShowItem(ItemInstance inst, ItemDataSo data, List<Vector2Int> absCalls, List<int> localIndices, int rotation = 0)
        {
            HideItem(inst); //기존 잔여물 제거

            if (tileTemplate is null || parentTransform is null || grid is null)
            {
                Debug.LogWarning("참조가 누락되었습니다.");
                return;
            }
            
            List<Image> tiles = new List<Image>(absCalls.Count); //타일 컨테이너
            //현재 데이터에 스프라이트가 존재한다면 데이터꺼 쓰고 아니면 임시 스프라이트 쓰기

            for (int i = 0; i < absCalls.Count; i++)
            {
                Vector2Int cell = absCalls[i];
                int idx = (i < localIndices.Count) ? localIndices[i] : i;

                Sprite sprite = null;
                if (data != null && data.cellSprites != null && idx >= 0 && idx < data.cellSprites.Count)
                {
                    //만약 데이터도 있고 스프라이트도 있으며 인덱스가 0보다 크고 적절하다면
                    sprite = data.cellSprites[idx]; //스프라이트 설정
                }

                if (sprite == null)
                {
                    sprite = fallbackSprite;
                }
                
                //모든 예외처리를 통과했다면 이미지 생성하기
                Image img = Instantiate(tileTemplate, parentTransform); //생성 (나중에 풀링으로 바꿈
                img.gameObject.SetActive(true); //이미지 켜주기
                img.raycastTarget = false; //레이케스트 제거 (불필요한 이벤트 발생 줄이기)
                img.sprite = sprite; //스프라이트 설정
                img.type = Image.Type.Simple; //타입은 그냥 심플로
                img.preserveAspect = false; //크기 딱맞게 조정

                RectTransform rt = (RectTransform)img.transform; //피벗에 맞게 위치 조정
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f); //셀 중앙값
                rt.sizeDelta = new Vector2(grid.cellSizePx, grid.cellSizePx); //사이즈를 현재 칸의 픽셀만큼 조정
                rt.anchoredPosition = grid.CellToAnchoredPos(cell); //셀 중심
                rt.localEulerAngles = new Vector3(0, 0, rotation);
                
                tiles.Add(img);
            }
            _rendered[inst.instanceId] = tiles; //모두 설정 끝나면 딕셔너리에 추가 (캐싱)
        }
        
        //이미 그려진 아이템을 새 셀 목록으로 이동
        public void MoveItem(ItemInstance inst, List<Vector2Int> absCells)
        {
            //이미 그려져 있다면
            if (!_rendered.TryGetValue(inst.instanceId, out List<Image> tiles))
                return;

            //칸 수가 변할 수도 있다면 ShowItem으로 다시 그리는 게 안전
            int n = Mathf.Min(tiles.Count, absCells.Count);
            for (int i = 0; i < n; i++)
            {
                RectTransform rt = (RectTransform)tiles[i].transform;
                rt.anchoredPosition = grid.CellToAnchoredPos(absCells[i]); //각 칸의 중심으로 이동
            }
        }

        //아이템 제거 시 타일도 제거
        public void HideItem(ItemInstance inst)
        {
            if (inst == null) return; //인스턴스가 널이라면 리턴
            if (_rendered.TryGetValue(inst.instanceId, out List<Image> tiles)) //아이디에 맞는 이미지 타일 리스트가 있다면
            {
                foreach (Image img in tiles) //모든 이미지 순회
                {
                    if (img != null) Destroy(img.gameObject); //이미지가 있다면 제거
                    _rendered.Remove(inst.instanceId); //딕셔너리에서도 제거
                }
            }
        }

        
        //전체 초기화
        public void ClearAll()
        {
            foreach (var kv in _rendered)
            {
                foreach (Image img in kv.Value)
                {
                    if (img != null) Destroy(img.gameObject);
                }
            }
            _rendered.Clear();
        }
    }
}