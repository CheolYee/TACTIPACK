using System.Collections.Generic;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items
{
    public abstract class ItemDataSo : ScriptableObject
    {
        [Header("References")]
        public string itemId; //아이템 고유 ID
        public string itemName; //아이템 이름
        public ItemClass itemClass; //어떤 클래스가 사용하는 아이템인가?
        public Sprite icon; //인벤토리 아이콘
        [TextArea] public string description; //아이템 설명
        
        [Header("Shop Settings")]
        public int price; //아이템 가격

        [Header("BindingSetting")]
        public bool isAllowedBinding;

        [Header("Default Shape Settings")]
        public Vector2Int shape = new(1, 1); //기본 사각형 크기

        [Header("Free Shape Settings")]
        public Vector2Int[] shapeOffsets; //개별 셀 오프셋 목록 (pivot 기준)
        public Vector2Int pivot = Vector2Int.zero;  //중심 기준점 (회전 기준)

        [Header("Image By Cell")]
        public List<Sprite> cellSprites;
        
        [Header("Consumable Settings")]
        [Tooltip("소모성 아이템 여부")]
        public bool isConsumable = false;

        [Tooltip("소모성일 때 한 인스턴스가 사용할 수 있는 최대 횟수")]
        public int maxUses = 1;


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
        
        #if UNITY_EDITOR
        private void OnValidate()
        {
            Vector2Int[] offs = GetShapeOffsets();
            int need = offs != null ? offs.Length : shape.x * shape.y;
            if (need < 0) need = 0;

            if (cellSprites == null) cellSprites = new List<Sprite>();
            if (cellSprites.Count < need) //만약 필요한 개수보다 이미지가 적다면
            {
                int add = need - cellSprites.Count; //추가할 개수 계산
                for (int i = 0; i < add; i++) cellSprites.Add(null); //나머지를 임시로 fallback이미지 사용
            }
            else if (cellSprites.Count > need)
            {
                cellSprites.RemoveRange(need, cellSprites.Count - need); //필요한 개수보다 이미지가 많이 들어있다면 지움
            }
        }
        #endif
    }

    [CreateAssetMenu(fileName = "newActiveItem", menuName = "SO/Item/ActiveItem", order = 0)]
    public abstract class ActiveItemDataSo : ItemDataSo
    {
        [Header("Active Item Settings")]
        [Tooltip("쿨타임 (턴 단위)")]
        public int cooldownTurns;
        
        [Tooltip("스킬 프리팹 (프로젝타일, 이펙트 등)")]
        public GameObject skillPrefab;
    }   

    [CreateAssetMenu(fileName = "newPassiveItem", menuName = "SO/Item/PassiveItem", order = 1)]
    public abstract class PassiveItemDataSo : ItemDataSo
    {
        [Header("Passive Item Settings")]
        [Tooltip("스탯 보정 (%)")]
        public int cooldownTurns;
        public int healthMulti;
        public int attackMulti;
        public float critMulti;
    }

    public enum ItemClass
    {
        Any = 0, //전체 공용
        Warrior = 1, //전사
        Mage = 2, //마법사
        Healer = 3 //힐러
    }
}