using System;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Attacks;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Attacks.Damages;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Managers;
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
        
        private float _baseMaxHealth; //캐릭터 원래 MaxHP
        private float _passiveHpRate; //패시브로 인한 추가 비율
        
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
            _baseMaxHealth = health;

            // 패시브 매니저가 이미 살아있다면, 그 값을 바로 반영
            var psm = PassiveStatManager.Instance;
            if (psm != null && _owner != null)
            {
                _passiveHpRate = psm.GetHpRateFor(_owner);
            }
            else
            {
                _passiveHpRate = 0f;
            }
            
            MaxHealth = CalcMaxHealth();
            _currentHealth = MaxHealth;
             
            if (healthBarPrefab != null)
            {
                _healthBarInstance = Instantiate(healthBarPrefab, transform);
                _healthBarInstance.Bind(_owner, this);
            }
             
            OnInitHealth?.Invoke(_currentHealth, _currentHealth, MaxHealth);
             
        }
        
        private float CalcMaxHealth()
        {
            return _baseMaxHealth * (1f + _passiveHpRate);
        }
        
        public void SetPassiveHpRate(float rate)
        {
            _passiveHpRate = rate;
            float newMax = CalcMaxHealth();
            RecalculateMaxHealthKeepCurrent(newMax);
        }
        
        public void RecalculateMaxHealthKeepCurrent(float newMaxHealth)
        {
            float prevHealth = _currentHealth;

            MaxHealth = newMaxHealth;
            // 현재 체력은 그대로 두되, 새 최대체력보다 크면 잘라줌
            _currentHealth = Mathf.Clamp(prevHealth, 0, MaxHealth);

            // UI 갱신용 이벤트
            OnHealthChange?.Invoke(prevHealth, _currentHealth, MaxHealth);
        }

        //체력 회복할 떄 사용함
        public void Heal(float amount)
        {
            if (amount <= 0f) return;
            float prevHealth = _currentHealth;
        
            _currentHealth = Mathf.Clamp(_currentHealth + amount, 0, MaxHealth);
            
            //힐 텍스트
            if (amount > 0f && DamageTextSpawner.Instance != null)
            {
                DamageTextSpawner.Instance.Spawn(
                    amount,
                    false,
                    transform.position,
                    DamageTextKind.Heal
                );
            }
        
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
            StatusEffectController statusController = null;
            
            if (_owner != null)
            {
                //베리어 있을 시 그냥 데미지 안들어오게 처리
                statusController = _owner.GetCompo<StatusEffectController>();
                if (statusController != null &&
                    statusController.TryBlockDamage(ref attackData))
                {
                    DamageTextSpawner.Instance.Spawn(
                        0,
                        false, 
                        transform.position, 
                        DamageTextKind.Barrier);
                    return;
                }
            }
            
            float prevHealth = _currentHealth; //이전체력을 맞기 전의 현재체력으로 설정
            //0보다 작지 않도록, 최대체력보다 크지 않도록 조정
            float newHealth = Mathf.Clamp(_currentHealth - attackData.Damage, 0, MaxHealth);
            
            //무적 체크
            if (statusController != null)
            {
                statusController.TryApplyLastStand(prevHealth, ref newHealth);
            }
            
            _currentHealth = newHealth;
            
            if (attackData.Damage > 0f && DamageTextSpawner.Instance != null)
            {
                DamageTextSpawner.Instance.Spawn(
                    attackData.Damage,
                    attackData.IsCritical,
                    transform.position);
            }

            //화상 추가피해
            if (_owner != null)
            {
                statusController = _owner.GetCompo<StatusEffectController>();
                statusController?.OnDamaged(attackData);
            }

            //맞았으니 이벤트 실행 (이전 체력, 현재 체력, 최대 체력)
            OnHealthChange?.Invoke(prevHealth, _currentHealth, MaxHealth);

            if (Mathf.Approximately(_currentHealth, 0)) //만약 0과 현재체력이 근사치라면 죽은것으로 판단
            {
                OnDeath?.Invoke(); //죽음 이벤트 발생
            }
        }
        
        public void ApplyDirectDamage(float damage, DamageTextKind kind = DamageTextKind.Normal)
        {
            float prevHealth = _currentHealth;
            float newHealth = Mathf.Clamp(_currentHealth - damage, 0, MaxHealth);
            
            //무적 체크
            if (_owner != null)
            {
                var statusController = _owner.GetCompo<StatusEffectController>();
                if (statusController != null)
                {
                    statusController.TryApplyLastStand(prevHealth, ref newHealth);
                }
            }

            _currentHealth = newHealth;
            
            //크리 유무 상관없으니까 그냥 false
            if (damage > 0f && DamageTextSpawner.Instance != null)
            {
                DamageTextSpawner.Instance.Spawn(
                    damage,
                    false,
                    transform.position,
                    kind
                );
            }

            OnHealthChange?.Invoke(prevHealth, _currentHealth, MaxHealth);
            if (Mathf.Approximately(_currentHealth, 0))
                OnDeath?.Invoke();
        }
    }
}