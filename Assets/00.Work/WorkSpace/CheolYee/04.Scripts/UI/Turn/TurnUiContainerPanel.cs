using System;
using System.Collections;
using System.Collections.Generic;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Agents;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Attacks.Skills;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Effects;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Events;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Creatures.Enemies;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Creatures.Players;
using UnityEngine;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Managers;
using DG.Tweening;
using UnityEngine.UI;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.UI.Turn
{
    public class TurnUiContainerPanel : MonoBehaviour
    {
        [Header("References")] 
        [SerializeField] private SkillBindingController skillBindingController;
        [SerializeField] private RectTransform content;
        [SerializeField] private TurnSlotUi slotPrefab; //실제 드래그되는 슬롯
        [SerializeField] private Canvas canvas;
        [SerializeField] private VerticalLayoutGroup layoutGroup;
        
        [Header("Limit")]
        [SerializeField] private int maxPlayers = 3;
        
        [Header("Enemies")]
        [SerializeField] private List<Enemy> enemies = new();
        
        public VerticalLayoutGroup LayoutGroup => layoutGroup;
        public RectTransform Content => content;
        
        private readonly List<TurnSlotUi> _slots = new();

        private bool _waitingForSkill;
        private Agent _waitingAgent;

        private void OnEnable()
        {
            //스킬 끝남 처리를 받기 위해 버스 구독
            Bus<SkillFinishedEvent>.OnEvent += OnSkillFinished;
        }

        private void OnDisable()
        {
            Bus<SkillFinishedEvent>.OnEvent -= OnSkillFinished;
        }

        private void Awake()
        {
            Build();
        }

        private void Build()
        {
            if (slotPrefab == null || content == null) return;

            List<Player> players = PartyManager.Instance.Players;
            if (players == null || players.Count == 0) return;
            
            int count = Math.Min(maxPlayers, players.Count);


            for (int i = 0; i < count; i++)
            {
                Player p = players[i];
                TurnSlotUi slot = Instantiate(slotPrefab, content);
                slot.name = $"[Turn UI Slot] : {p.CharacterData.name}";
                
                slot.Initialize(this, canvas);
                slot.BindPlayer(p);
                
                var skillSlot = slot.GetSkillSlot();
                if (skillSlot != null)
                {
                    skillBindingController.RegisterSkillSlot(skillSlot);
                }
                
                _slots.Add(slot);
            }
        }

        private void Clear()
        { 
            foreach (var s in _slots)
            {
                if (s != null) Destroy(s.gameObject);
            }
            _slots.Clear();
        }
        public List<Player> GetCurrentOrder()
        {
            List<Player> order = new List<Player>(_slots.Count);
            foreach (var s in _slots)
            {
                order.Add(s.BoundPlayer);
            }
            return order;
        }
        
        public void SwapSlots(TurnSlotUi a, TurnSlotUi b)
        {
            if (a == null || b == null || a == b) return;

            int indexA = _slots.IndexOf(a);
            int indexB = _slots.IndexOf(b);

            if (indexA == -1 || indexB == -1 || indexA == indexB)
                return;

            //리스트 상에서 순서 교체
            (_slots[indexA], _slots[indexB]) = (_slots[indexB], _slots[indexA]);

            //리스트에서 교체된거 현실반영
            for (int i = 0; i < _slots.Count; i++)
            {
                _slots[i].transform.SetSiblingIndex(i);
            }

            PlaySwapAnimation(a.transform, b.transform);
        }

        public void DebugPrintBoundSkills()
        {
            if (_slots == null || _slots.Count == 0)
            {
                Debug.LogWarning("TurnUiContainerPanel: 실행할 슬롯이 없습니다.");
                return;
            }

            StartCoroutine(RunBattleTurnSequence());

        }
        
        private IEnumerator RunBattleTurnSequence()
        {
            Debug.Log("===== [TurnUiContainerPanel] 턴 실행 시작 =====");

            // 1) 플레이어 턴
            yield return RunPlayerTurnSequence();

            // 2) 에너미 턴
            yield return RunEnemyTurnSequence();

            Debug.Log("===== [TurnUiContainerPanel] 라운드 종료 =====");
        }


        //모든 플레이어의 턴 시퀀스를 순차적으로 실행합니다.
        private IEnumerator RunPlayerTurnSequence()
        {
            if (_slots == null || _slots.Count == 0)
            {
                Debug.LogWarning("실행할 슬롯이 없습니다.");
                yield break;
            }

            for (int i = 0; i < _slots.Count; i++)
            {
                //슬롯 가져왔는데 없으면 넘기기
                TurnSlotUi slot = _slots[i];
                if (slot == null) continue;
                
                Player player = slot.BoundPlayer;
                if (player == null) continue; //플레이어가 없어도 넘기기
                
                Debug.Log($"[TurnUiContainerPanel] 턴 {i + 1} : {player.name} 스킬 실행");
                
                //이번 턴의 사용자 지정 후 스킬 사용중이라고 표시
                _waitingForSkill = true;
                _waitingAgent = player;
                
                //이번 턴 스킬 실행
                slot.ExecuteBoundSkill();
                
                //이벤트가 와서 스킬이 모두 끝날 때 까지 대기
                yield return new WaitUntil(() => !_waitingForSkill);
            }
            
            _waitingAgent = null;
        }
        private IEnumerator RunEnemyTurnSequence()
        {
            Debug.Log("[TurnUiContainerPanel] 에너미 턴에 진입했씁니다.");
            if (enemies == null || enemies.Count == 0)
            {
                Debug.Log("[TurnUiContainerPanel] 에너미가 없어 에너미 턴을 스킵합니다.");
                yield break;
            }

            // 에너미들이 공격할 대상(플레이어들)을 한 번 세팅해둔다.
            /*var playerHealths = PartyManager.Instance.Players
                .Where(p => p != null && p.Health != null) // Health 프로퍼티 있다고 가정
                .Select(p => p.Health)
                .ToList();

            BattleSkillManager.Instance.SetEnemyTargets(playerHealths);*/

            for (int i = 0; i < enemies.Count; i++)
            {
                Enemy enemy = enemies[i];
                if (enemy == null || enemy.Health == null)
                    continue;

                // 죽은 적은 스킵
                if (enemy.IsDead)
                    continue;

                Debug.Log($"[TurnUiContainerPanel] 에너미 턴 {i + 1} : {enemy.name} 스킬 실행");

                _waitingForSkill = true;
                _waitingAgent = enemy;
                
                enemy.StartTurn();

                yield return new WaitUntil(() => !_waitingForSkill);
            }

            _waitingAgent = null;
            Debug.Log("[TurnUiContainerPanel] 에너미 턴 실행 끝");
        }

        //스킬 종료 이벤트 버스가 오면 여기서 받기
        private void OnSkillFinished(SkillFinishedEvent evt)
        {
            if (!_waitingForSkill)
                return;

            if (_waitingAgent != null && evt.User != _waitingAgent)
                return;

            _waitingForSkill = false;
        }
        
        private void PlaySwapAnimation(Transform a, Transform b)
        {
            float upScale = 1.1f;
            float downScale = 0.9f;
            float duration = 0.12f;

            Vector3 originalScaleA = a.localScale;
            Vector3 originalScaleB = b.localScale;

            Sequence seq = DOTween.Sequence();

            seq.Join(a.DOScale(originalScaleA * upScale, duration));
            seq.Join(b.DOScale(originalScaleB * downScale, duration));

            seq.Append(a.DOScale(originalScaleA, duration));
            seq.Join(b.DOScale(originalScaleB, duration));
        }
    }
}