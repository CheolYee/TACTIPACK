using _00.Work.WorkSpace.CheolYee._04.Scripts.Creatures.Players;
using UnityEngine;
using UnityEngine.UI;

namespace _00.Work.WorkSpace.Hyeonjin0209._01.Scripts.System.Character
{
    public class CharacterSelect : MonoBehaviour
    {

        public CharacterClass characterType;
        private Image _image;

        private void Awake()
        {
            _image = GetComponent<Image>();
        }

        public void PointerDown()
        {

            var chmanager = CharacterSelectManager.Instance;

            if (chmanager.choiceCharacter.Contains(this))
            {
                chmanager.choiceCharacter.Remove(this);
                _image.color = Color.white;
                return;
            }
            if (chmanager.choiceCharacter.Count >= 3) return;

            if(!chmanager.choiceCharacter.Contains(this))
            {
                bool isSelected = chmanager.SelectCharacter(this);
                if (isSelected)
                    _image.color = Color.gray;
            }
        }
    }
}