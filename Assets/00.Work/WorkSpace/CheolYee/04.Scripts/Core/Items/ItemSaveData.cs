using System.Collections.Generic;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items
{
    [System.Serializable]
    public class ItemSaveData
    {
        public int backpackWidth; //백팩 가로 칸 수
        public int backpackHeight; //백팩 세로 칸 수
        public List<ItemInstance> items = new List<ItemInstance>(); //모든 아이템 인스턴스 리스트

        //저장 시: ItemInstance의 rotation, isInBackpack, backpackPosition 모두 포함됨
    }
}