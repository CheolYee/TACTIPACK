using _00.Work.WorkSpace.CheolYee._04.Scripts.Agents;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Attacks;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.ItemTypes.ActiveItems;
using _00.Work.WorkSpace.CheolYee._04.Scripts.FSMSystem;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Managers;
using _00.Work.WorkSpace.CheolYee._04.Scripts.UI.Turn;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Creatures.Players
{
    public class Player : Agent, IAttackable
    {
        //기본 캐릭터 데이터
        [field: SerializeField] public PlayerDefaultData CharacterData { get; private set; }
        [SerializeField] private StateListSo playerStateList;
        
        private AgentStateMachine _stateMachine;
        
        public AgentState CurrentState => _stateMachine.CurrentState;

        protected override void AfterInitializeComponent()
        {
            base.AfterInitializeComponent();
            _stateMachine = new AgentStateMachine(this, playerStateList.states);
        }
        public void SetupCharacter(PlayerDefaultData data)
        {
            CharacterData = data;
            CharacterInit();
        }

        private void CharacterInit()
        {
            Debug.Assert(CharacterData != null, $"캐릭터 데이터가 없습니다! {name}");
            
            Renderer.InitRenderer(CharacterData.AnimatorController, CharacterData.CharacterOffset);
            Health.InitHealth(CharacterData.MaxHp);
            Health.HealthBarInstance.SetName(CharacterData.CharacterName);
            
            
            BattleSkillManager.Instance.RegisterPlayer(Health);
        }

        private void Start()
        {
            ChangeState(PlayerStates.IDLE);
        }

        private void Update()
        {
            _stateMachine.UpdateMachine();
        }

        public void ChangeState(PlayerStates newState)
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
            
            if (TurnUiContainerPanel.Instance != null)
            {
                TurnUiContainerPanel.Instance.RemovePlayerSlot(this);
            }

            
            ChangeState(PlayerStates.DEATH);
        }

        public void Attack(AttackItemSo item)
        {
            actionData.CurrentAttackItem = item;
            
            ChangeState(PlayerStates.ATTACK);
        }

    }
}