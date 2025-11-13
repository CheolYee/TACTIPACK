using _00.Work.WorkSpace.CheolYee._04.Scripts.Creatures.Players;
using _00.Work.WorkSpace.CheolYee._04.Scripts.FSMSystem;
using UnityEngine;
using UnityEngine.UI;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.UI.Turn
{
    public class TurnSlotUi : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image icon;
        
        public Player BoundPlayer {get; private set;}

        public void BindPlayer(Player player)
        {
            BoundPlayer = player;

            PlayerDefaultData data = player.CharacterData;
            if (data != null && icon != null)
                icon.sprite = data.CharacterIcon;
        }
    }
}