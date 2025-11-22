using _00.Work.Resource.Scripts.Managers;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.ItemTypes;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.UI.SideItem;
using _00.Work.WorkSpace.CheolYee._04.Scripts.UI;
using _00.Work.WorkSpace.JaeHun._01._Scrpits;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _00.Work.WorkSpace.Soso7194._04.Scripts.UI
{
    public class ItemRandomSelect : MonoBehaviour
    {
        [SerializeField] private AllItemDatabase itemDatabase;
        
        [SerializeField] private Image itemImage;
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TooltipTarget tooltipTarget;
        
        
        private ItemDataSo _selectedItem;

        private void OnEnable()
        {
            int randomIndex = Random.Range(0, itemDatabase.AllItems.Count);
            
            _selectedItem = itemDatabase.AllItems[randomIndex];
            tooltipTarget.SetText(_selectedItem.itemName, _selectedItem.description);
            
            SetItem();
        }

        private void SetItem()
        {
            itemImage.sprite = _selectedItem.icon;
            itemNameText.text = _selectedItem.itemName;

            SideInventoryManager.Instance.AddItem(_selectedItem);
        }
    }
}
