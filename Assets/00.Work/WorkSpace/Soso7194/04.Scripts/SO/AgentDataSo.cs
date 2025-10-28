using UnityEngine;

namespace _00.Work.WorkSpace.Soso7194._04.Scripts.SO
{
    public class AgentDataSo : ScriptableObject
    {
        [field: SerializeField] public CharacterEnum characterType { get; set; }
        [field: SerializeField] public Sprite characterSprite { get; set; }
        [field: SerializeField] public int maxHP { get; set; } // 최대 HP
        [field: SerializeField] public int attackPower { get; set; }
        [field: SerializeField] public int criticalHitChance { get; set; } // 치명타 확률
    }
}