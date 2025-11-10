using _00.Work.WorkSpace.CheolYee._04.Scripts.Agents;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Creatures.Players
{
    public enum CharacterClass
    {
        Warrior = 1, //전사
        Mage = 2, //마법사
        Healer = 3 //힐러
    }
    
    [CreateAssetMenu(fileName = "PlayerDefaultData", menuName = "SO/Player/DefaultData", order = 0)]
    public class PlayerDefaultData : ScriptableObject
    {
        [Header("Info")]
        [field: SerializeField] public string CharacterName {get; private set;} //캐릭터의 이름
        [field: SerializeField] public int CharacterId {get; private set;} //캐릭터의 아이디
        [field: SerializeField] public CharacterClass CharacterClass {get; private set;} //캐릭터의 직업 클래스
        
        [Header("Settings")]
        [field: SerializeField] public float MaxHp {get; private set;} //HP
        [field: SerializeField] public float DefaultDamage {get; private set;} //기본 데미지
        [field: SerializeField] public float DefaultCritChance {get; private set;} //크리티컬 확률
    }
}