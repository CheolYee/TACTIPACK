// 공통 캐릭터 인터페이스
public interface ICharacter
{
    CharacterDataSO Data { get; set; }
    int CurrentHP { get; set; }

    void Attack();
    void EatHealItem();
}

// 직업별 세부 인터페이스
public interface IHealer : ICharacter
{
    void HealerSkill();
}

public interface IWizard : ICharacter
{
    void WizardSkill();
}

public interface IWarrior : ICharacter
{
    void WarriorSkill();
}
