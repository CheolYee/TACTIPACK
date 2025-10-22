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

            List<Image> tiles = new List<Image>(absCalls.Count);
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
                
                Image img = Instantiate(tileTemplate, parentTransform);
                img.gameObject.SetActive(true);
                img.raycastTarget = false;
                img.sprite = sprite;
                img.type = Image.Type.Simple;
                img.preserveAspect = false;

                RectTransform rt = (RectTransform)img.transform;
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(grid.cellSizePx, grid.cellSizePx);
                rt.anchoredPosition = grid.CellToAnchoredPos(cell);
                rt.localEulerAngles = new Vector3(0,0,rotation);

                tiles.Add(img);
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