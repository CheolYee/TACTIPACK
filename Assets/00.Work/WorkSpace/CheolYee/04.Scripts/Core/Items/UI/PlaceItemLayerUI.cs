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
            HideItem(inst);
            if (tileTemplate == null || parentTransform == null || grid == null)
            {
                Debug.LogError("참조가 덜 되었습니다."); 
                return;
            }

            //셀을 담을 타일 이미지 리스트
            List<Image> tiles = new List<Image>(absCalls.Count);
            
            //셀 개수만큼 생성
            for (int i = 0; i < absCalls.Count; i++)
            {
                int idx = (localIndices != null && i < localIndices.Count) ? localIndices[i] : i;
                Vector2Int cell = absCalls[i];


                Sprite sprite = null;
                if (data != null && data.cellSprites != null && idx >= 0 && idx < data.cellSprites.Count)
                {
                    sprite = data.cellSprites[idx];
                }

                if (sprite == null && data != null)
                {
                    sprite = fallbackSprite;
                }
                
                //이미지 설정
                Image img = Instantiate(tileTemplate, parentTransform); //이미지 생성
                img.gameObject.SetActive(true); //이미지 켜기
                img.raycastTarget = false; //쓸모없는 레이케스트 끄기 (최적화)
                img.sprite = sprite; //스프라이트 설정
                img.type = Image.Type.Simple; //이미지 타입 설정
                img.preserveAspect = false; //이미지 타일에 꽉차게

                //위치 설정
                RectTransform rt = (RectTransform)img.transform; //위치 (명시적 형변환)
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f); //엥커 조정
                rt.sizeDelta = new Vector2(grid.cellSizePx, grid.cellSizePx); //사이즈 직접 조정 (한칸 사이즈)
                rt.anchoredPosition = grid.CellToAnchoredPos(cell); //셀 포지션 자동 설정
                rt.localEulerAngles = new Vector3(0,0,rotation); //각도를 현재 돌려진 각도만큼 돌리기

                tiles.Add(img); //다 만들었으면 
            }

            _rendered[inst.instanceId] = tiles;
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