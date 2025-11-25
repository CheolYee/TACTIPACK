using System;
using System.Collections.Generic;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Stages.Maps;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Save
{
    [Serializable]
    public class GameSaveData
    {
        public PartySaveData party; //파티 구성 + 현재 HP
        public MapProgressSaveData map; //맵 진행도
        public GridInventorySaveData gridInventory; //인벤토리(그리드)
        public SideInventorySaveData sideInventory; //인벤토리(사이드)
    }

    // 파티 세이브 데이터 (어떤 캐릭터들이 있고, 현재 HP 얼마인지)
    [Serializable]
    public class PartySaveData
    {
        public List<PlayerSaveData> players = new();
    }

    [Serializable]
    public class PlayerSaveData
    {
        public int characterId; //PlayerDefaultData.CharacterId
        public float currentHp; //AgentHealth.CurrentHealth
    }

    //맵 진행도 세이브 데이터
    [Serializable]
    public class MapProgressSaveData
    {
        public int currentChapter;          //현재 챕터
        public int lastClearedMapId;        //마지막으로 클리어한 맵 id
        public List<MapNodeSaveEntry> nodes = new();  //각 맵의 상태 목록
    }

    [Serializable]
    public class MapNodeSaveEntry
    {
        public int mapId; //MapSo.mapId
        public MapNodeState state; //Locked / Available / Cleared
    }

    // 그리드 인벤토리 세이브 데이터
    [Serializable]
    public class GridInventorySaveData
    {
        public int width;   // 그리드 가로 칸 수 (혹시 나중에 바뀔 수도 있으니 저장)
        public int height;  // 그리드 세로 칸 수
        public List<GridItemSaveEntry> items = new();
    }

    /// 그리드 위에 깔린 각 아이템 하나의 정보
    [Serializable]
    public class GridItemSaveEntry
    {
        public string dataId; // ItemDataSo.itemId
        public int anchorX; // 앵커 셀 X
        public int anchorY; // 앵커 셀 Y
        public int rotation; // 회전(0, 90, 180, 270)
        public int remainingCooldown; //쿨타임
        public int remainingUses; // -1이면 비소모성(무제한)
    }


    // 사이드 인벤토리 세이브 데이터
    [Serializable]
    public class SideInventorySaveData
    {
        public List<SideInventoryItemSaveEntry> items = new();
    }

    [Serializable]
    public class SideInventoryItemSaveEntry
    {
        public string dataId; // ItemDataSo.itemId
        public int count;     // 현재 스택 개수
    }
    
    [Serializable]
    public class ShopSaveData
    {
        public int coin;
    }
}