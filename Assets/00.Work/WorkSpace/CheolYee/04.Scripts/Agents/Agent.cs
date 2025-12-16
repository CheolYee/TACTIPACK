using System;
using System.Collections.Generic;
using System.Linq;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Attacks;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Attacks.Damages;
using UnityEngine;
using UnityEngine.Events;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Agents
{
    public abstract class Agent : MonoBehaviour, IDamageable
    {
        protected Dictionary<Type, IAgentComponent> ComponentDict; //Agent 컴포넌트 시스템을 담을 딕셔너리
        
        public AgentHealth Health { get; protected set; } //체력 시스템 (모든 생명체는 체력이 존재함
        public AgentRenderer Renderer { get; protected set; }
        public StatusEffectController StatusEffectController { get; protected set; }
        public bool IsDead { get; protected set; } //죽었나 안 죽었나 표시
        public AgentActionData actionData; //공격 데이터를 전달하기 위한 구조체
        
        public UnityEvent onAgentHit; //공격 받았을 떄 실행
        public UnityEvent onAgentDeath; //죽었을 떄 실행
        
        protected virtual void Awake()
        {
            //자식 오브젝트에서 IAgentComponent를 구현하고 있는 모든 컴포넌트를 가져와 딕셔너리에 저장한다.
            ComponentDict = GetComponentsInChildren<IAgentComponent>(true).ToDictionary(compo => compo.GetType());

            InitComponents();
            AfterInitializeComponent();
        }
        //모든 AgentComponent를 초기화 시킵니다.
        protected virtual void InitComponents()
        {
            foreach (IAgentComponent compo in ComponentDict.Values)
            {
                compo.Initialize(this);
            }
            
            Health = GetCompo<AgentHealth>();
            Renderer = GetCompo<AgentRenderer>();
            StatusEffectController = GetCompo<StatusEffectController>();
        }

        //모든 AgentComponent를 초기화 시킨 후 실행될 두번째 초기 설정 함수입니다.
        protected virtual void AfterInitializeComponent()
        {
            ComponentDict.Values.OfType<IAfterInitialize>()
                .ToList()
                .ForEach(compo => compo.AfterInitialize());

            Health.OnDeath += HandleAgentDeath;
            Health.OnHealthChange += HandleHealthChange;
        }
        
        //체력이 변경되었다면 이벤트를 발생시킨다.
        protected virtual void HandleHealthChange(float prevHealth, float currentHealth, float maxHealth)
        {
            if (prevHealth > currentHealth)
                onAgentHit.Invoke();
        }
        
        //체력 이벤트에 구독해놨던 것을 바인드 해제합니다.
        protected virtual void OnDestroy()
        {
            Health.OnDeath -= HandleAgentDeath;
            Health.OnHealthChange -= HandleHealthChange;
        }
        
        //상속받은 다른 객체들이 구현
        protected virtual void HandleAgentDeath()
        {
            
        }

        //IAgentComponent 한정 GetComponent와 비슷한 기능을 하는 메서드입니다.
        public T GetCompo<T>()
        {
            //T타입이 정확이 딕셔너리에 키로 들어가 있다면
            //그 T 타입을 반환합니다.
            if (ComponentDict.TryGetValue(typeof(T), out IAgentComponent component)
                && component is T compo)
            {
                return compo;
            }
            
            //만약 T가 정확히 키가 아니라면 (인터페이스나 상속으로 찾는다면)
            //딕셔너리에 T로 대입 가능한지 검사해 찾은 첫번째 타입을 반환합니다.
            IAgentComponent findComponent = ComponentDict.Values.FirstOrDefault(type => type is T);
            if(findComponent is T findCompo)
                return findCompo;
            
            //그럼에도 찾지 못했다면 기본 타입을 반환
            return default(T);
        }

        //Agent에 데미지를 주기 위함
        public void ApplyDamage(DamageContainer attackData)
        {
            actionData.LastAttackData = attackData;
            
            if (attackData.Damage <= 0) return;
            
            if (StatusEffectController != null &&
                StatusEffectController.TryBlockDamage(ref attackData))
            {
                return;
            }
            
            Health.ApplyDamage(attackData);
        }
        
        //크확 계산기
        public virtual float GetBaseCritChance()
        {
            // 기본은 0 (필요한 쪽에서 오버라이드)
            return 0f;
        }
        
        
    }
}