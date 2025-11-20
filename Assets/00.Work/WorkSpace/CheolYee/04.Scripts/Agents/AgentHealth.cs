using System;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Attacks;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Attacks.Damages;
using _00.Work.WorkSpace.CheolYee._04.Scripts.UI.HealthBar;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Agents
{
    public class AgentHealth : MonoBehaviour, IAgentComponent, IDamageable
    {
        [field: SerializeField] public float MaxHealth { get; private set; } //최대체력
        [SerializeField] private HealthBarUi healthBarPrefab;
        public float CurrentHealth => _currentHealth; //현재체력 읽을 수 있는 프로퍼티
        public Agent Owner => _owner;
        public HealthBarUi HealthBarInstance => _healthBarInstance;

        private float _currentHealth; //현재체력
        private Agent _owner; //소유자
        private HealthBarUi _healthBarInstance; //체력바
        
        
        
        //데미지등 다양한 체력 데이터를 표시하기 위해 데이터를 넘겨줌
        public delegate void HealthChange(float prevHealth, float currentHealth, float maxHealth);

        public event HealthChange OnInitHealth;
        public event HealthChange OnHealthChange; //체력 변경 이벤트
        public event Action OnDeath; //죽음 이벤트

        public void Initialize(Agent agent) //초기화 함수
        {
            _owner = agent;
        }

        public void InitHealth(float health)
        {
             MaxHealth = health;
             _currentHealth = MaxHealth;
             
             if (healthBarPrefab != null)
             {
                 _healthBarInstance = Instantiate(healthBarPrefab, transform);
                 _healthBarInstance.Bind(_owner, this);
             }
             
             OnInitHealth?.Invoke(_currentHealth, _currentHealth, MaxHealth);
             
        }

        //체력 회복할 떄 사용함
        public void Heal(float amount)
        {
            if (amount <= 0f) return;
            float prevHealth = _currentHealth;
        
            _currentHealth = Mathf.Clamp(_currentHealth + amount, 0, MaxHealth);
        
            // 그냥 체력만 바꾸고, 맞았을 때 OnDamaged 같은 건 안 탐
            OnHealthChange?.Invoke(prevHealth, _currentHealth, MaxHealth);

            if (Mathf.Approximately(_currentHealth, 0))
            {
                OnDeath?.Invoke();
            }
        }
        
        //데미지를 주기 위한 메서드 (데미지만 줌)
        public void ApplyDamage(DamageContainer attackData)
        {
            if (_owner != null)
            {
                //베리어 있을 시 그냥 데미지 안들어오게 처리
                StatusEffectController statusController = _owner.GetCompo<StatusEffectController>();
                if (statusController != null &&
                    statusController.TryBlockDamage(ref attackData))
                {
                    return;
                }
            }
            
            float prevHealth = _currentHealth; //이전체력을 맞기 전의 현재체력으로 설정
            //0보다 작지 않도록, 최대체력보다 크지 않도록 조정
            _currentHealth = Mathf.Clamp(_currentHealth - attackData.Damage, 0, MaxHealth);

            if (_owner != null)
            {
                StatusEffectController statusController = _owner.GetCompo<StatusEffectController>();
                statusController?.OnDamaged(attackData);
            }

            //맞았으니 이벤트 실행 (이전 체력, 현재 체력, 최대 체력)
            OnHealthChange?.Invoke(prevHealth, _currentHealth, MaxHealth);

            if (Mathf.Approximately(_currentHealth, 0)) //만약 0과 현재체력이 근사치라면 죽은것으로 판단
            {
                OnDeath?.Invoke(); //죽음 이벤트 발생
            }
        }
        
        public void ApplyDirectDamage(float damage)
        {
            float prevHealth = _currentHealth;
            _currentHealth = Mathf.Clamp(_currentHealth - damage, 0, MaxHealth);

            OnHealthChange?.Invoke(prevHealth, _currentHealth, MaxHealth);
            if (Mathf.Approximately(_currentHealth, 0))
                OnDeath?.Invoke();
        }
    }
}