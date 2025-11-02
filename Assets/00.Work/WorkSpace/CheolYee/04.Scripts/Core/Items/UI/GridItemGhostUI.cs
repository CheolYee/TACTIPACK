using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.UI
{
    public class GridItemGhostUI : MonoBehaviour
    {
        [Header("Visual")] 
        public RectTransform root; //유령 타일들을 배치할 부모 위치
        public Image tileTemplate; //슬롯 표현용 이미지 프리팹
        public Color okColor = new(0f, 1f, 0f, 0.45f); //가능(초록 반투명)
        public Color badColor = new Color(1f, 0f, 0f, 0.45f); //불가능(빨강 반투명)
        public Vector2 cellSize =  new Vector2(64f, 64f); //한 칸 크기
        
        private readonly List<Image> _tiles = new(); //이미지 캐싱

        private void Awake()
        {
            if (root == null) root = (RectTransform)transform; //루트 미할당 시 자신 사용
            if (tileTemplate != null) tileTemplate.gameObject.SetActive(false); //템플릿은 비활성
        }
        
        //타일 수에 맞춰서 생성과 활성화하기
        private void EnsureTiles(int count)
        {
            while (_tiles.Count < count)
            {
                Image inst = Instantiate(tileTemplate, root); //템플릿으로 이미지 생성
                inst.gameObject.SetActive(true); //새 타일은 활성
                _tiles.Add(inst); //캐시에 추가
            }

            //초과하면 숨기기
            for (int i = 0; i < _tiles.Count; i++)
            {
                _tiles[i].gameObject.SetActive(i < count);
            }
        }

        public void SetCellsAnchored(List<Vector2> anchoredPositions)
        {
            EnsureTiles(anchoredPositions.Count); //타일 수 보정
            for (int i = 0; i < anchoredPositions.Count; i++)
            {
                Image img = _tiles[i]; //타일
                RectTransform rt = (RectTransform)img.transform; //캐스팅
                rt.anchoredPosition = anchoredPositions[i]; //셀 포지션 지정
                rt.sizeDelta = cellSize; //셀 사이즈 설정 
            }
        }
        
        //가능/불가 색상 지정
        public void SetOk(bool ok)
        {
            Color c = ok ? okColor : badColor; // 상태에 따른 색
            foreach (var t in _tiles)
                t.color = c; // 일괄 적용
        }

        //유령 감추기
        public void Hide()
        {
            foreach (var t in _tiles)
                t.gameObject.SetActive(false);  // 전부 숨김
        }
    }
}