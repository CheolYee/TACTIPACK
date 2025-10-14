
using Unity.VisualScripting;
using UnityEngine;


public class Healer :MonoBehaviour, ICharacter, IHealer
{
    [field:SerializeField] public CharacterDataSO Data { get ; set ; }
    public int CurrentHP { get ; set ;}

    private void Start()
    {
        CurrentHP = Data.maxHP;
    }
    public void Attack()
    {
     
    }

    public void EatHealItem()
    {
       
    }

    public void HealerSkill()
    {
        
    }
}
