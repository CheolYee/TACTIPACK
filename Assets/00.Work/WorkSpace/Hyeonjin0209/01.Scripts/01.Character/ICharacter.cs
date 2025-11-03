// 공통 캐릭터 인터페이스

using UnityEngine;

public interface ICharacter
{
    CharacterDataSO Data { get; set; }
    Sprite characterSprite { get;}
    public int maxHP { get;} // 최대 HP
    int attackPower { get;}
    int criticalHitChance { get;} // 치명타 확률
    void Attack();
}

public interface IPlayer : ICharacter
{
    CharacterEnum characterType { get;}
    void EatHealItem();
}

// 직업별 세부 인터페이스
public interface IHealer : IPlayer
{
    void HealerSkill();
}

public interface IWizard : IPlayer
{
    void WizardSkill();
}

public interface IWarrior : IPlayer
{
    void WarriorSkill();
}

public interface IEnemy : ICharacter
{
    
}
