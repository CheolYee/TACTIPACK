using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.UI
{
    public class GridVisualSlotsUI : MonoBehaviour
    {
        [Header("Refs")] 
        public GridInventoryUIManager grid; //그리드 좌표, 치수 치환용 
        public RectTransform parentRect; //슬롯이 담길 부모 오브젝트 위치
        public Image slotTemplate; //슬롯 이미지 템플릿(프리팹)

        [Header("Colors")] 
        public Color emptyColor = new Color(1,1,1,0.06f); //빈칸 색
        public Color occupedColor = new Color(1,1,1,0.16f); //차지 색
        
        private Image[] _slots; //[x + y * width]로 접근

        private void Awake()
        {
            BuildSlots(); //실행 시 슬롯 생성
        }

        public void BuildSlots()
        {
            if (grid == null || parentRect == null || slotTemplate == null) return;
            
            //기존에 자식이 남아있다면 즉시 제거
            for (int i = parentRect.childCount - 1; i >= 0; i--)
                DestroyImmediate(parentRect.GetChild(i).gameObject);
            
            _slots = new Image[grid.width * grid.height]; //총 칸 수만큼 이미지 준비
            slotTemplate.gameObject.SetActive(false); //템플릿 비활성

            
            //그리드만큼 모든 슬롯 인스턴스 생성
            int idx = 0;
            for (int y = 0; y < grid.height; y++)
            for (int x = 0; x < grid.width; x++, idx++)
            {
                //슬롯 인스턴스 생성
                Image img = Instantiate(slotTemplate, parentRect);
                img.gameObject.SetActive(true); //켜주기
                img.raycastTarget = false; //드래그 간섭 방지
                RectTransform rt = (RectTransform)img.transform; //위치 가져오기
                rt.sizeDelta =  new Vector2(grid.cellSizePx, grid.cellSizePx); //사이즈 설정
                rt.anchoredPosition = grid.CellToAnchoredPos(new Vector2Int(x, y)); //위치 설정 보정
                img.color = emptyColor; //빈 공간 색으로 설정
                _slots[idx] = img; //현재 슬롯에 캐싱
            }
        }

        //색상을 재설정한다.
        public void RefreshColors()
        {
            //만약 슬롯이 없다면 리턴
            if (_slots == null) return;
            
            
            //모든 슬롯을 돌아서 차있는지 아닌지 확인 후 설정한다.
            int idx = 0;
            for (int y = 0; y < grid.height; y++)
            for (int x = 0; x < grid.width; x++, idx++)
            {
                bool occ = grid.IsOccupied(new Vector2Int(x, y));
                _slots[idx].color = occ ? occupedColor : emptyColor;
            }
        }
    }
}