using System;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Events;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items
{
    [Serializable]
    public class ItemInstance
    {
        public string instanceId;           // 고유 인스턴스 ID
        public string dataId;               // 연결된 ItemDataSo ID

        public int rotation;                // 회전 상태 (0, 90, 180, 270)
        
        [SerializeField] private int remainingCooldownTurns; // 남은 쿨타임
        public int RemainingCooldownTurns => remainingCooldownTurns;
        public bool IsOnCooldown => remainingCooldownTurns > 0;

        //생성자 (기본)
        public ItemInstance(string data)
        {
            instanceId = Guid.NewGuid().ToString();
            dataId = data;
            rotation = 0;
            remainingCooldownTurns = 0;
        }

        public void StartCooldown(int cooldownTurns)
        {
            if (cooldownTurns <= 0)
            {
                remainingCooldownTurns = 0;
                Bus<ItemCooldownChangedEvent>.Raise(new ItemCooldownChangedEvent(this, remainingCooldownTurns));
                return;
            }
            
            remainingCooldownTurns = cooldownTurns;

            Bus<ItemCooldownStartedEvent>.Raise(
                new ItemCooldownStartedEvent(this, remainingCooldownTurns));

            Bus<ItemCooldownChangedEvent>.Raise(
                new ItemCooldownChangedEvent(this, remainingCooldownTurns));
        }

        //턴 1회 경과 시 호출
        public void TickCooldownOneTurn()
        {
            if (remainingCooldownTurns <= 0) return;
            
            remainingCooldownTurns--;
            if (remainingCooldownTurns < 0)
            {
                remainingCooldownTurns = 0;
            }
            
            Bus<ItemCooldownChangedEvent>.Raise(
                new ItemCooldownChangedEvent(this, remainingCooldownTurns));
        }

        //아이템의 실제 셀 좌표를 계산하는 메서드
        public Vector2Int[] GetOccupiedCells(ItemDataSo data)
        {
            var offsets = data.GetShapeOffsets();
            return GridShapeUtil.GetRotatedOffsets(offsets, rotation);
        }
    }
}