using _00.Work.WorkSpace.CheolYee._04.Scripts.Agents;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Attacks;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Effects;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Events;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.ItemTypes.ActiveItems;
using _00.Work.WorkSpace.CheolYee._04.Scripts.FSMSystem;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Managers;
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
        [SerializeField] private AttackItemSo currentSkill; 
        
        private AgentStateMachine _stateMachine;
        
        protected override void AfterInitializeComponent()
        {
            base.AfterInitializeComponent();
            _stateMachine = new AgentStateMachine(this, enemyStateList.states);
        }
        
        public void SetupEnemy(EnemyDefaultData data)
        {
            EnemyData = data;
            EnemyInit();
        }

        private void EnemyInit()
        {
            Debug.Assert(EnemyData != null, $"캐릭터 데이터가 없습니다! {name}");
            
            Renderer.InitRenderer(EnemyData.AnimatorController, EnemyData.CharacterOffset);
            Health.InitHealth(EnemyData.MaxHp);
            Health.HealthBarInstance.SetName(EnemyData.EnemyName);
            
            BattleSkillManager.Instance.RegisterEnemy(Health);
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
        
        protected override void HandleAgentDeath()
        {
            if (IsDead) return;
            IsDead = true;
            
            onAgentDeath?.Invoke();

            if (BattleSkillManager.Instance != null)
            {
                BattleSkillManager.Instance.Unregister(Health);
            }
            
            ChangeState(EnemyStates.DEATH);
        }


        public void Attack(AttackItemSo ite, ItemInstance inst = null)
        {
            ChangeState(EnemyStates.ATTACK);
        }

        private void SetRandomAttackSkill()
        {
            int randomAttack = Random.Range(0, EnemyData.Attacks.Count);
            actionData.CurrentAttackItem = EnemyData.Attacks[randomAttack];
            currentSkill = EnemyData.Attacks[randomAttack];
        }

        public void StartTurn()
        {
            SetRandomAttackSkill();
            
            if (currentSkill == null)
            {
                Debug.LogWarning($"{name} : defaultSkill 이 비어있어서 턴을 바로 종료합니다.");
                Bus<SkillFinishedEvent>.Raise(new SkillFinishedEvent(this, null));
                return;
            }

            Attack(currentSkill);
        }
    }
}