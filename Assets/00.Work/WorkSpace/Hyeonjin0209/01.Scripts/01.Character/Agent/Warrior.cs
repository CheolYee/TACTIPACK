using UnityEngine;

public class Warrior : MonoBehaviour, ICharacter , IWarrior
{
    [field: SerializeField] public CharacterDataSO Data { get ; set ; }
    
    public CharacterEnum characterType { get; }
    public Sprite characterSprite { get; }
    public int maxHP { get; private set; }
    public int attackPower { get; private set; }
    public int criticalHitChance { get; private set;}

    public void Attack()
    {
        maxHP = Data.maxHP;
    }

    public void EatHealItem()
    {
       
    }

    public void WarriorSkill()
    {
     
    }
}
