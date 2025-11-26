using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items;
using UnityEngine;
using UnityEngine.Serialization;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Creatures.Players
{
    public enum CharacterClass
    {
        Warrior = 1, //전사
        Mage = 2, //마법사
        Healer = 3 //힐러
    }
    
    [CreateAssetMenu(fileName = "PlayerDefaultData", menuName = "SO/Character/PlayerDefaultData", order = 0)]
    public class PlayerDefaultData : ScriptableObject
    {
        [Header("Info")]
        [field: SerializeField] public string CharacterName {get; private set;} //캐릭터의 이름
        [field: SerializeField] public int CharacterId {get; private set;} //캐릭터의 아이디
        [TextArea] 
        public string characterDesc; //캐릭터의 설명
        [field: SerializeField] public CharacterClass CharacterClass {get; private set;} //캐릭터의 직업 클래스
        [field: SerializeField] public RuntimeAnimatorController AnimatorController {get; private set;}
        
        [field: SerializeField] public Vector2 CharacterOffset {get; private set;}
        
        [Header("UI")]
        [field: SerializeField] public Sprite CharacterIcon {get; private set;}
        [field: SerializeField] public Sprite CharacterBoxIcon {get; private set;}
        
        [Header("Settings")]
        [field: SerializeField] public float MaxHp {get; private set;} //HP
        [field: SerializeField] public float DefaultCritChance {get; private set;} //크리티컬 확률
        [field: SerializeField] public ItemDataSo StartItem {get; private set;} //시작할 떄 가지고 있을 하나의 아이템
    }
    
    
}