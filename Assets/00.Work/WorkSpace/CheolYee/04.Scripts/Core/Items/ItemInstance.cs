using System;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items
{
    [Serializable]
    public class ItemInstance
    {
        public string instanceId;           // 고유 인스턴스 ID
        public string dataId;               // 연결된 ItemDataSo ID

        public bool isInBackpack;           // 백팩(그리드)에 들어있는가
        public Vector2Int backpackPosition; // 백팩 내 기준 위치(anchor)
        public int rotation;                // 회전 상태 (0, 90, 180, 270)

        //생성자 (기본)
        public ItemInstance(string data)
        {
            instanceId = Guid.NewGuid().ToString();
            dataId = data;
            isInBackpack = false;
            backpackPosition = Vector2Int.zero;
            rotation = 0;
        }

        //아이템의 실제 셀 좌표를 계산하는 메서드 (GridShapeUtil 사용)
        public Vector2Int[] GetOccupiedCells(ItemDataSo data)
        {
            var offsets = data.GetShapeOffsets();
            return GridShapeUtil.GetRotatedOffsets(offsets, rotation);
        }
    }
}