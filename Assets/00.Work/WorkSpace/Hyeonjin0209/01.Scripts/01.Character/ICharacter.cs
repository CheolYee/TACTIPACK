// 공통 캐릭터 인터페이스
public interface ICharacter
{
    CharacterDataSO Data { get; set; }
    int CurrentHP { get; set; }

    void Attack();
}

public interface IPlayer : ICharacter
{
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
