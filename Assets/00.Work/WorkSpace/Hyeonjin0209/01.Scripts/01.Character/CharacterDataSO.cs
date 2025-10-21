using UnityEngine;

[CreateAssetMenu(fileName = "CharacterDataSO", menuName = " CharacterSO/CharacterDataSO")]
public class CharacterDataSO : ScriptableObject
{
    [field: SerializeField] public CharacterEnum characterType { get; set; }
    [field: SerializeField] public Sprite characterSprite { get; set; }
    [field: SerializeField] public int HP { get; set; }
    [field: SerializeField] public int maxHP { get; set; } // 최대 HP
    [field: SerializeField] public int attackPower { get; set; }
    [field: SerializeField] public int criticalHitChance { get; set; } // 치명타 확률
    //힐러 힐량
    [field: SerializeField] public int healAmount { get; set; }
}
