using System;
using System.Collections;
using _00.Work.Resource.Scripts.Managers;
using _00.Work.Resource.Scripts.Utils;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Agents;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.AnimatorSystem;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Attacks;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Effects;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Events;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.ItemTypes.ActiveItems;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.UI;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Managers;
using _00.Work.WorkSpace.CheolYee._04.Scripts.FSMSystem;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Managers;
using _00.Work.WorkSpace.CheolYee._04.Scripts.UI;
using _00.Work.WorkSpace.CheolYee._04.Scripts.UI.Turn;
using DG.Tweening;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Creatures.Players.PlayerState
{
    public class PlayerAttackState : PlayerState
    {
        private AgentRenderer _agentRenderer; //렌더러
        private AttackExecutor _attackExecutor; //공격 실행기
        BattleSkillManager _skillManager; //스킬 메니저
        
        private Vector3 _spawnPosition; //초기 위치
        private Tween _moveTween; //현재 진행 중인 이동 트윈
        private Coroutine _stateFlow;
        
        private bool _fired; //애니메이션 Fire 이벤트 수신 플래그
        private bool _isExiting; //Exit 진행 중(취소 플래그)
        
        private ItemDatabase _itemDatabase;
        
        public PlayerAttackState(Agent agent, AnimParamSo stateParam) : base(agent, stateParam)
        {
        }

        public override void Enter()
        {
            if (Player.StatusEffectController.IsStunned)
            {
                Debug.Log($"{Player.name} 는 기절 상태라 AttackState 진입을 즉시 종료합니다.");

                Bus<SkillFinishedEvent>.Raise(new SkillFinishedEvent(Agent, Agent.actionData.CurrentAttackItem));
                Player.ChangeState(PlayerStates.IDLE);
                return;
            }
            
            //내부 트리거 콜 초기화
            IsTriggerCall = false;
            _isExiting = false;
            _fired = false;
            
            //가져왔는데 하나라도 예외가 나면 IDLE로 돌아가도록 처리한다.
            try
            {
                _agentRenderer = Player.GetCompo<AgentRenderer>();
                _attackExecutor = Player.GetCompo<AttackExecutor>();
                _skillManager = Object.FindFirstObjectByType<BattleSkillManager>();
                _itemDatabase = Object.FindFirstObjectByType<ItemDatabase>();
            }
            catch (NullReferenceException e)
            {
                Debug.LogException(e);
                Debug.LogError($"{Player.name}의 AttackState가 설정될 수 없습니다.");
                Player.ChangeState(PlayerStates.IDLE);
                return;
            }
            
            //애니메이션 이벤트 구독
            _agentRenderer.OnAnimationFire += OnFire; //애니메이션 시작시점
            _agentRenderer.OnAnimationEnd += OnEnd; //애니메이션 종료 지점
            
            //스폰위치를 Agent의 위치로 설정
            _spawnPosition = Agent.transform.position;

            _stateFlow = Agent.StartCoroutine(StateFlow());
        }

        private IEnumerator StateFlow()
        {
            //기본 데이터 가져오기
            AttackItemSo item = Agent.actionData.CurrentAttackItem;
            SkillContent ctx = _skillManager.BuildContext(Agent, item, item.DefaultStance);

            if (ctx.Targets.Count <= 0)
            {
                //타겟이 없으면 그냥 돌아와잇
                Bus<SkillFinishedEvent>.Raise(new SkillFinishedEvent(Agent, item));
                Player.ChangeState(PlayerStates.IDLE);
                yield break;
            }
            
            SkillNameLabelUI.Instance?.ShowSkillName(item.itemName);
            ctx.User.Renderer.SetAttackSortingHighlight(true);
            
            //리플렉션 빌드
            StanceInvoker.BuildMap(this);

            //리플렉션으로 메서드 자동 매핑하여 트윈 실행
            _moveTween = StanceInvoker.Invoke(this, ctx.Stance, ctx);
            if (_moveTween != null) //실행 성공했으면
            {
                //대기
                yield return _moveTween.WaitForCompletion();
                //만약 대기 도중 나가기 됐다면 탈출
                if (_isExiting) yield break;
            }

            //대기 모션 다 끝났으면 공격 실행
            _agentRenderer.SetParam(StateParam, true);

            _fired = false;
            //fire이나 탈출 타이밍까지 대기
            yield return new WaitUntil(() => _fired || _isExiting);

            //실제 공격 실행기로 실행
            yield return Agent.StartCoroutine(_attackExecutor.Perform(item, ctx));
            if (_isExiting) yield break;
            
            var inst = Agent.actionData.CurrentItemInst;
            if (inst != null && item != null)
            {
                // 쿨타임 처리
                if (item.cooldownTurns > 0)
                {
                    inst.StartCooldown(item.cooldownTurns);
                    ItemCooldownManager.Instance.Register(inst);
                }

                //소모성 처리
                if (_itemDatabase != null)
                {
                    var data = _itemDatabase.GetItemById(inst.dataId);
                    if (data != null && data.isConsumable)
                    {
                        //혹시 InitUses를 못 받은 경우를 대비한 안전장치
                        if (!inst.HasLimitedUses)
                        {
                            inst.InitUses(data.maxUses);
                        }

                        inst.ConsumeUse();
                        
                        //숫자갱신
                        var grid = Object.FindFirstObjectByType<GridInventoryUIManager>();
                        grid?.UpdateConsumableUsesVisual(inst);

                        if (inst.IsDepleted)
                        {
                            //그리드에서 제거
                            grid?.Remove(inst);
                            
                            //뒷배경 다시 복구
                            GridInventoryUIController.Instance?.gridSlots?.RefreshColors();

                            //턴 UI 바인딩 해제
                            var panel = TurnUiContainerPanel.Instance;
                            panel?.ClearBindingForItem(inst);
                        }
                    }
                }
            }
            
            SkillCameraManager.Instance?.SetAnchor(CamAnchor.Target, ctx.User.transform);
            //만약 캐릭터가 움직이는 상태였다면
            if (ctx.Stance == AttackStance.StepForward || ctx.Stance == AttackStance.DashToTarget)
            {
                //원래 자리로 돌아온다
                _moveTween?.Kill();
                _moveTween = Agent.transform.DOMove(_spawnPosition, 0.15f).SetEase(Ease.InSine);
                yield return _moveTween.WaitForCompletion();
                if (_isExiting) yield break;
            }
            
            ctx.User.Renderer.SetAttackSortingHighlight(false);

            HudManager.Instance?.ShowAll();
            yield return new WaitForSeconds(0.5f);
            
            //턴메니저에 보낼 스킬 종료 이벤트
            Bus<SkillFinishedEvent>.Raise(new SkillFinishedEvent(Agent, item));
            //공격이 모두 끝날 시 IDle로 전환
            
            Player.ChangeState(PlayerStates.IDLE);
        }

        public override void Exit()
        {
            _isExiting = true;
            
            //퇴장 시 구독 해제 처리
            if (_agentRenderer != null)
            {
                _agentRenderer.OnAnimationFire -= OnFire;
                _agentRenderer.OnAnimationEnd  -= OnEnd;
            }
            
            //코루틴 중지
            if (_stateFlow != null)
            {
                Agent.StopCoroutine(_stateFlow);
                _stateFlow = null;
            }
            
            // 트윈 정리
            _moveTween?.Kill();
            _moveTween = null;

            base.Exit();
        }
        
        private void OnFire()
        {
            _fired = true;
        }

        private void OnEnd()
        {
            
        }
        
        [StanceHandler(AttackStance.Stationary)]
        private Tween HandleStationary(SkillContent ctx)
        {
            SkillCameraManager.Instance.SetAnchor(CamAnchor.Target, ctx.User.transform);
            SkillCameraManager.Instance.ZoomTo(7f, 0.3f);
            return null;
        }
        
        [StanceHandler(AttackStance.StepForward)]
        private Tween HandleStepForward(SkillContent ctx)
        {
            var from = Agent.transform.position;
            var dir  = (ctx.CastPoint - from).normalized;
            var step = from + dir * ctx.ApproachOffset;

            var seq = DOTween.Sequence();
            seq.AppendInterval(0.5f);
            seq.Append(Agent.transform.DOMove(step, 0.5f).SetEase(Ease.OutSine));
            SkillCameraManager.Instance.SetAnchor(CamAnchor.Target, ctx.User.transform);
            SkillCameraManager.Instance.ZoomTo(7f, 0.3f);
            return seq;
        }

        [StanceHandler(AttackStance.DashToTarget)]
        private Tween HandleDashToTarget(SkillContent ctx)
        {
            SkillCameraManager.Instance.SetAnchor(CamAnchor.Target, ctx.User.transform);
            SkillCameraManager.Instance.ZoomTo(7f, 1f);
            
            var from = Agent.transform.position;
            var dir  = (ctx.CastPoint - from).normalized;
            
            var destination = ctx.CastPoint;

            if (Mathf.Abs(ctx.ApproachOffset) > 0.01f)
            {
                destination = ctx.CastPoint - dir * ctx.ApproachOffset;
            }

            return Agent.transform.DOMove(destination, 0.25f).SetEase(Ease.OutSine);
        }
        
    }
}