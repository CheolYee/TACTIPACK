using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items
{
    public abstract class ItemDataSo : ScriptableObject
    {
        [Header("기본 정보")]
        public string itemId; //아이템 고유 ID
        public string itemName; //아이템 이름
        public ItemClass itemClass; //어떤 클래스가 사용하는 아이템인가?
        public Sprite icon; //인벤토리 아이콘

        [Header("기본 형태 설정")]
        public Vector2Int shape = new(1, 1); //기본 사각형 크기
        public bool rotatable = true; //회전 가능한지 여부

        [Header("테트리스형 모양 설정 (shapeOffsets가 존재하면 shape는 무시됨)")]
        public Vector2Int[] shapeOffsets; //개별 셀 오프셋 목록 (pivot 기준)
        public Vector2Int pivot = Vector2Int.zero;  //중심 기준점 (회전 기준)

        [Header("시각적 표현")]
        public Sprite gridPreviewSprite; //그리드 인벤토리에서 표시할 미리보기 스프라이트

        [Header("기타")]
        [TextArea] public string description; //아이템 설명

        //shapeOffsets가 비어 있으면 사각형 셀로 변환하여 반환
        public Vector2Int[] GetShapeOffsets()
        {
            if (shapeOffsets != null && shapeOffsets.Length > 0)
                return shapeOffsets;

            //사각형 형태 자동 생성
            Vector2Int[] rectOffsets = new Vector2Int[shape.x * shape.y];
            int index = 0;
            for (int y = 0; y < shape.y; y++)
            {
                for (int x = 0; x < shape.x; x++)
                {
                    rectOffsets[index++] = new Vector2Int(x, y);
                }
            }
            return rectOffsets;
        }
    }

    [CreateAssetMenu(fileName = "newActiveItem", menuName = "SO/Item/ActiveItem", order = 0)]
    public abstract class ActiveItemDataSo : ItemDataSo
    {
        [Header("Active Item Settings")]
        [Tooltip("쿨타임 (턴 단위)")]
        public int cooldownTurns;
        
        [Tooltip("스킬 프리팹 (프로젝타일, 이펙트 등)")]
        public GameObject skillPrefab;

        public virtual void Activate(GameObject user, GameObject target = null) {}
    }

    [CreateAssetMenu(fileName = "newPassiveItem", menuName = "SO/Item/PassiveItem", order = 1)]
    public abstract class PassiveItemDataSo : ItemDataSo
    {
        [Header("Passive Item Settings")]
        [Tooltip("스탯 보정 (예: 체력 +10, 공격력 +2)")]
        public int bonusHealth;
        public int bonusAttack;
    }

    public enum ItemClass
    {
        Any = 0, //전체 공용
        Warrior = 1, //전사
        Mage = 2, //마법사
        Healer = 3 //힐러
    }
}