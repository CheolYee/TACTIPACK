using UnityEngine;

[CreateAssetMenu(fileName = "CharacterDataSO", menuName = " CharacterSO/CharacterDataSO")]
public class CharacterDataSO : ScriptableObject
{
    public CharacterEnum characterType { get; set; }
    public Sprite characterSprite { get; set; }
    public int hP { get; set; } 
    public int maxHP { get; set; } // 최대 HP
    public int attackPower { get; set; }
    public int criticalHitChance { get; set; } // 치명타 확률
    //힐러 힐량
    public int healAmount { get; set; }
}
