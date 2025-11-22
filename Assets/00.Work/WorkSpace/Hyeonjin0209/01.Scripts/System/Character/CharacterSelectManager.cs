using System.Collections.Generic;
using UnityEngine;

namespace _00.Work.WorkSpace.Hyeonjin0209._01.Scripts.System.Character
{
    public class CharacterSelectManager : MonoBehaviour
    {
        public static CharacterSelectManager Instance;

        private void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
            }
            else Destroy(gameObject);
        }
        public List<CharacterSelect> choiceCharacter = new List<CharacterSelect>();

        public bool SelectCharacter(CharacterSelect type)
        {
            if (choiceCharacter.Contains(type))
            {
                choiceCharacter.Remove(type);
                return false;
            }
            else
            {
                if (choiceCharacter.Count < 3)
                    choiceCharacter.Add(type);
                return true;
            }     
        }
    }
}
