using _00.Work.WorkSpace.CheolYee._04.Scripts.Agents;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Attacks;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Effects;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Events;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.ItemTypes.ActiveItems;
using _00.Work.WorkSpace.CheolYee._04.Scripts.FSMSystem;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Creatures.Enemies
{
    
    
    public class Enemy : Agent, IAttackable
    {
        [Header("Reference")] 
        [field: SerializeField] public EnemyDefaultData EnemyData { get; private set; }

        [Header("States")] 
        [SerializeField] private StateListSo enemyStateList;
        
        [Header("Skill")]
        [SerializeField] private AttackItemSo defaultSkill; //에너미가 사용할 고유 스킬 한종
        
        public AgentState CurrentState => _stateMachine.CurrentState;
        public AttackItemSo CurrentSkill => defaultSkill;
        
        private AgentStateMachine _stateMachine;
        
        protected override void AfterInitializeComponent()
        {
            base.AfterInitializeComponent();
            _stateMachine = new AgentStateMachine(this, enemyStateList.states);
            
            EnemyInit();
        }

        private void EnemyInit()
        {
            Debug.Assert(EnemyData != null, $"캐릭터 데이터가 없습니다! {name}");
            
            Health.InitHealth(EnemyData.MaxHp);
        }
        
        private void Start()
        {
            ChangeState(EnemyStates.IDLE);
        }

        private void Update()
        {
            _stateMachine.UpdateMachine();
        }

        public void ChangeState(EnemyStates newState)
        {
            _stateMachine.ChangeState((int)newState);
        }

        public void Attack(AttackItemSo item)
        {
            ActionData.CurrentAttackItem = item;
            
            ChangeState(EnemyStates.ATTACK);
        }
        
        public void StartTurn()
        {
            if (defaultSkill == null)
            {
                Debug.LogWarning($"{name} : defaultSkill 이 비어있어서 턴을 바로 종료합니다.");
                // 아무것도 안 하고 턴만 넘기고 싶다면 SkillFinishedEvent를 바로 쏴도 됨
                Bus<SkillFinishedEvent>.Raise(new SkillFinishedEvent(this, null));
                return;
            }

            Attack(defaultSkill);
        }
    }
}