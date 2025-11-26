using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Creatures.Players;
using UnityEngine;
using UnityEngine.UI;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.UI
{
    public class ItemPreviewSlot : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TooltipTarget tooltipTarget;

        public void Initialize(ItemDataSo itemData)
        {
            icon.sprite = itemData.icon;
            tooltipTarget.SetText(itemData.itemName,itemData.description);
        }
    }
}