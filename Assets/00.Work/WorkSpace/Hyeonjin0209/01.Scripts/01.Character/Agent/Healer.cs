
using Unity.VisualScripting;
using UnityEngine;


public class Healer : MonoBehaviour, IHealer
{
    [field:SerializeField] public CharacterDataSO Data { get ; set ; }
    public int CurrentHP { get ; set ;}
    
    public CharacterEnum characterType { get; }
    public Sprite characterSprite { get; }
    public int maxHP { get; private set; }
    public int attackPower { get; private set; }
    public int criticalHitChance { get; private set;}

    private void Start()
    {
        maxHP = Data.maxHP;
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
