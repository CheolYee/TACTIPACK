using System;
using System.Collections.Generic;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.UI
{
    public class PlaceItemLayerUI : MonoBehaviour
    {
        [Header("References")] 
        public GridInventoryManager grid; //좌표변환용
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
        public void ShowItem(ItemInstance inst, ItemDataSo data, List<Vector2Int> absCalls)
        {
            HideItem(inst); //기존 잔여물 제거
            
            List<Image> tiles = new List<Image>(absCalls.Count); //타일 컨테이너
            //현재 데이터에 스프라이트가 존재한다면 데이터꺼 쓰고 아니면 임시 스프라이트 쓰기
            Sprite sprite = data.gridPreviewSprite != null ? data.gridPreviewSprite : fallbackSprite;

            foreach (Vector2Int cell in absCalls)
            {
                Image img = Instantiate(tileTemplate, parentTransform); //타일 한개 생성
                img.gameObject.SetActive(true);
                img.raycastTarget = false; //마우스 포인터 간섭 없애기
                img.sprite = sprite; //스프라이트 설정
                RectTransform rt = (RectTransform)img.transform;
                rt.sizeDelta = new Vector2(grid.cellSizePx, grid.cellSizePx); //이미지 사이즈 맞추기
                tiles.Add(img); //세팅 끝나면 타일에 추가
            }
            _rendered[inst.instanceId] = tiles; //모두 설정 끝나면 딕셔너리에 추가
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