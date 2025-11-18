using System;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.UI
{
    public class HoverCellUI : MonoBehaviour
    {
        [Header("References")] 
        public GridInventoryUIManager grid; //좌표변환
        public RectTransform hoverRect; //하이라이트 박스
        public Image hoverImage; //색과 알파 관리
        public Color color = new Color(1f, 1f, 1f, 0.08f);

        private void Start()
        {
            if (hoverImage != null) hoverImage.color = color; //있다면 색상 적용
            Hide();
        }

        public void ShowAt(Vector2Int cell)
        {
            if (hoverRect == null) return;
            hoverRect.sizeDelta = new Vector2(grid.cellSizePx, grid.cellSizePx); //셀 크기
            hoverRect.anchoredPosition = grid.CellToAnchoredPos(cell); //위치 설정
            hoverRect.gameObject.SetActive(true); //보이기
        }

        public void Hide()
        {
            if (hoverImage != null) hoverRect.gameObject.SetActive(false); //숨기기
        }
    }
}