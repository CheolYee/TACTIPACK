using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.ItemTypes;
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
        
        private ItemDataSo _selectedItem;

        private void OnEnable()
        {
            int randomIndex = Random.Range(0, itemDatabase.AllItems.Count);
            
            _selectedItem = itemDatabase.AllItems[randomIndex];
            
            SetItem();
        }

        private void SetItem()
        {
            itemImage.sprite = _selectedItem.icon;
            itemNameText.text = _selectedItem.itemName;
        }
    }
}
