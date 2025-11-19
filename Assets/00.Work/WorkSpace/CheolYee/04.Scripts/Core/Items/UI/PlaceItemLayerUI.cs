using System.Collections.Generic;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.UI
{
    public class PlaceItemLayerUI : MonoBehaviour
    {
        [Header("References")] 
        public GridInventoryUIManager grid;       // 좌표 변환용
        public RectTransform parentTransform;     // 렌더 부모
        public Image tileTemplate;                // 한 칸 타일
        public Sprite fallbackSprite;             // 프리뷰 없을 때 대체

        // key: 인스턴스 아이디, value: 이 아이템을 표현하는 타일 이미지들
        private readonly Dictionary<string, List<Image>> _rendered = new();
        
        //쿨타임 비율
        private readonly Dictionary<string, float> _tintRatios = new();

        private void Awake()
        {
            if (tileTemplate != null)
                tileTemplate.gameObject.SetActive(false); // 템플릿 꺼두기
        }

        /// <summary>
        /// 아이템을 그리드 상에 그려준다.
        /// </summary>
        public void ShowItem(
            ItemInstance inst,
            ItemDataSo data,
            List<Vector2Int> absCells,
            List<int> localIndices,
            int rotation = 0)
        {
            HideItem(inst); // 혹시 이전에 그려져 있던 거 정리

            if (tileTemplate == null || parentTransform == null || grid == null)
            {
                Debug.LogError("PlaceItemLayerUI: 참조가 덜 되었습니다.");
                return;
            }
            
            string key = inst.instanceId;
            List<Image> tiles = new List<Image>(absCells.Count);

            for (int i = 0; i < absCells.Count; i++)
            {
                int idx = localIndices != null && i < localIndices.Count ? localIndices[i] : i;
                Vector2Int cell = absCells[i];

                // 셀 스프라이트 선택
                Sprite sprite = null;
                if (data != null && data.cellSprites != null && idx >= 0 && idx < data.cellSprites.Count)
                {
                    sprite = data.cellSprites[idx];
                }

                if (sprite == null && data != null)
                {
                    sprite = fallbackSprite;
                }

                // 이미지 생성
                Image img = Instantiate(tileTemplate, parentTransform);
                img.gameObject.SetActive(true);
                img.raycastTarget = false;
                img.sprite = sprite;
                img.type = Image.Type.Simple;
                img.preserveAspect = false;

                // 위치 / 크기 / 회전 설정
                RectTransform rt = (RectTransform)img.transform;
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(grid.cellSizePx, grid.cellSizePx);
                rt.anchoredPosition = grid.CellToAnchoredPos(cell);
                rt.localEulerAngles = new Vector3(0, 0, rotation);
                
                if (_tintRatios.TryGetValue(key, out float ratio))
                {
                    ApplyTintToImage(img, ratio);
                }

                tiles.Add(img);
            }

            _rendered[inst.instanceId] = tiles;
        }
        
        private void ApplyTintToImage(Image img, float ratio)
        {
            if (img == null) return;
            ratio = Mathf.Clamp01(ratio);

            // 완전 까맣지는 않게: 0.25 ~ 1.0 사이 밝기
            float brightness = Mathf.Lerp(0.5f, 1f, 1f - ratio);
            float alpha = Mathf.Lerp(0.8f, 1f, 1f - ratio);

            var c = img.color;
            c.r = brightness;
            c.g = brightness;
            c.b = brightness;
            c.a = alpha;
            img.color = c;
        }

        // 쿨타임 비율에 따라 아이템을 어둡고 투명하게 하기
        public void SetItemCooldownTint(ItemInstance inst, float ratio)
        {
            if (inst == null) return;
            if (!_rendered.TryGetValue(inst.instanceId, out var images) || images == null) return;

            ratio = Mathf.Clamp01(ratio);
            string key = inst.instanceId;

            if (ratio <= 0f)
            {
                // 쿨타임 끝이면 기록 삭제
                _tintRatios.Remove(key);
            }
            else
            {
                _tintRatios[key] = ratio;
            }

            if (_rendered.TryGetValue(key, out var tiles) && tiles != null)
            {
                foreach (var img in tiles)
                {
                    ApplyTintToImage(img, ratio);
                }
            }
        }

        // 특정 인스턴스를 그리는 모든 타일 제거
        public void HideItem(ItemInstance inst)
        {
            if (inst == null) return;
            string key = inst.instanceId;

            if (_rendered.TryGetValue(key, out List<Image> tiles))
            {
                foreach (Image img in tiles)
                {
                    if (img != null) Destroy(img.gameObject);
                }
                _rendered.Remove(key);
            }
        }
        public void ClearItemTint(ItemInstance inst)
        {
            if (inst == null) return;
            _tintRatios.Remove(inst.instanceId);
        }
        
        
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
            _tintRatios.Clear();
        }
    }
}
