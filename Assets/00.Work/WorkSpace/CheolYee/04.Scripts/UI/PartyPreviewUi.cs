using _00.Work.WorkSpace.CheolYee._04.Scripts.Creatures.Players;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.UI
{
    public class PartyPreviewUi : MonoBehaviour
    {
        [SerializeField] private PartyPreviewSlot[] slots;

        /// <summary>
        /// index 슬롯에 해당 캐릭터 프리뷰를 표시
        /// </summary>
        public void SetSlot(int index, PlayerDefaultData data)
        {
            if (slots == null) return;
            if (index < 0 || index >= slots.Length) return;
            if (slots[index] == null) return;

            if (data == null)
                slots[index].Hide();
            else
                slots[index].Show(data);
        }

        /// <summary>
        /// 전체 비우기 (필요시)
        /// </summary>
        public void ClearAll()
        {
            if (slots == null) return;
            foreach (var slot in slots)
            {
                if (slot != null)
                    slot.Hide();
            }
        }
    }
}