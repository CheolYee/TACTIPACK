using _00.Work.Resource.Scripts.Managers;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Creatures.Players;
using _00.Work.WorkSpace.CheolYee._04.Scripts.UI;
using UnityEngine;
using UnityEngine.UI;

namespace _00.Work.WorkSpace.Hyeonjin0209._01.Scripts.System.Character
{
    public class CharacterSelect : MonoBehaviour
    {

        [Header("Character Data")]
        [SerializeField] private PlayerDefaultData characterData;
        [SerializeField] private TooltipTarget tooltipTarget;
        [SerializeField] private ItemPreviewSlot itemPreviewSlot;
        
        private Image _image;
        
        public PlayerDefaultData CharacterData => characterData;
        public int CharacterId => characterData != null ? characterData.CharacterId : 0;
        
        private void Awake()
        {
            _image = GetComponent<Image>();
            _image.sprite = characterData.CharacterBoxIcon;
            
            tooltipTarget.SetText(characterData.CharacterName, characterData.characterDesc);
            itemPreviewSlot.Initialize(characterData.StartItem);
        }

        public void PointerDown()
        {
            var chManager = CharacterSelectManager.Instance;
            if (chManager == null)
                return;

            SoundManager.Instance.PlaySfx(SfxId.UiConfirm);
            
            bool selected = chManager.SelectCharacter(this);

            if (_image != null)
            {
                _image.color = selected ? Color.gray : Color.white;
            }
        }
    }
}