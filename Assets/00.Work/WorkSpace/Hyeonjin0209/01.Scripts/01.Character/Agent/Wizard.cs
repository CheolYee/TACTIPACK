using UnityEngine;



public class Wizard : MonoBehaviour, ICharacter, IWizard
{
    [field: SerializeField] public CharacterDataSO Data { get ; set; }
    public int CurrentHP { get; set; }

    private void Start()
    {
        Data.maxHP = 70;
        CurrentHP = Data.maxHP;
    }
    public void Attack()
    {
        
    }

    public void EatHealItem()
    {
        
    }

    public void WizardSkill()
    {
        
    }
}
