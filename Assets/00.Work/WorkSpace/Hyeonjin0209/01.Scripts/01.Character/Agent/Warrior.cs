using UnityEngine;

public class Warrior : MonoBehaviour, ICharacter , IWarrior
{
    [field: SerializeField] public CharacterDataSO Data { get ; set ; }
    public int CurrentHP { get ; set; }

    public void Attack()
    {
        Data.maxHP = 150;
        CurrentHP = Data.maxHP;
    }

    public void EatHealItem()
    {
       
    }

    public void WarriorSkill()
    {
     
    }
}
