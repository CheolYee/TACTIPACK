using _00.Work.WorkSpace.CheolYee._04.Scripts.Agents;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Attacks;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.ItemTypes.ActiveItems;
using _00.Work.WorkSpace.CheolYee._04.Scripts.FSMSystem;
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
            
            CharacterInit();
        }

        private void CharacterInit()
        {
            Debug.Assert(CharacterData != null, $"캐릭터 데이터가 없습니다! {name}");
            
            Health.InitHealth(CharacterData.MaxHp);
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

        public void Attack(AttackItemSo item)
        {
            ActionData.CurrentAttackItem = item;
            
            ChangeState(PlayerStates.ATTACK);
        }
    }
}