using System;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Attacks;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Agents
{
    public class AgentHealth : MonoBehaviour, IAgentComponent, IDamageable
    {
        [field: SerializeField] public float MaxHealth { get; private set; } //최대체력

        private float _currentHealth; //현재체력
        private Agent _owner; //소유자

        //데미지등 다양한 체력 데이터를 표시하기 위해 데이터를 넘겨줌
        public delegate void HealthChange(float prevHealth, float currentHealth, float maxHealth); 
        public event HealthChange OnHealthChange; //체력 변경 이벤트
        public event Action OnDeath; //죽음 이벤트

        public void Initialize(Agent agent) //초기화 함수
        {
            _owner = agent;
            _currentHealth = MaxHealth;
        }

        public void InitHealth(float health)
        {
             MaxHealth = health;
             _currentHealth = MaxHealth;
        }

        //데미지를 주기 위한 메서드 (데미지만 줌)
        public void ApplyDamage(AttackDataSo attackData)
        {
            float prevHealth = _currentHealth; //이전체력을 맞기 전의 현재체력으로 설정
            //0보다 작지 않도록, 최대체력보다 크지 않도록 조정
            _currentHealth = Mathf.Clamp(_currentHealth - attackData.Damage, 0, MaxHealth);

            //맞았으니 이벤트 실행 (이전 체력, 현재 체력, 최대 체력)
            OnHealthChange?.Invoke(prevHealth, _currentHealth, MaxHealth);

            if (Mathf.Approximately(_currentHealth, 0)) //만약 0과 현재체력이 근사치라면 죽은것으로 판단
            {
                OnDeath?.Invoke(); //죽음 이벤트 발생
            }
        }
    }
}