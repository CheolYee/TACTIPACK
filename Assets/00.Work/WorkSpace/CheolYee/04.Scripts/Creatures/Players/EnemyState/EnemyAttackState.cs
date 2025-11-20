using System;
using System.Collections;
using _00.Work.Resource.Scripts.Managers;
using _00.Work.Resource.Scripts.Utils;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Agents;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.AnimatorSystem;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Attacks;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Effects;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Events;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.ItemTypes.ActiveItems;
using _00.Work.WorkSpace.CheolYee._04.Scripts.FSMSystem;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Managers;
using DG.Tweening;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Creatures.Players.EnemyState
{
    public class EnemyAttackState : EnemyState
    {
        private AgentRenderer _agentRenderer; //렌더러
        private AttackExecutor _attackExecutor; //공격 실행기
        BattleSkillManager _skillManager; //스킬 메니저
        
        private Vector3 _spawnPosition; //초기 위치
        private Tween _moveTween; //현재 진행 중인 이동 트윈
        private Coroutine _stateFlow;
        
        private bool _fired; //애니메이션 Fire 이벤트 수신 플래그
        private bool _isExiting; //Exit 진행 중(취소 플래그)
        
        public EnemyAttackState(Agent agent, AnimParamSo stateParam) : base(agent, stateParam)
        {
        }
        
        public override void Enter()
        {
            if (Enemy.StatusEffectController.IsStunned)
            {
                Debug.Log($"{Enemy.EnemyData.EnemyName} 는 기절 상태라 AttackState 진입을 즉시 종료합니다.");

                Bus<SkillFinishedEvent>.Raise(new SkillFinishedEvent(Agent, Agent.actionData.CurrentAttackItem));
                Enemy.ChangeState(EnemyStates.IDLE);
                return;
            }
            
            //내부 트리거 콜 초기화
            IsTriggerCall = false;
            _isExiting = false;
            _fired = false;
            
            //가져왔는데 하나라도 예외가 나면 IDLE로 돌아가도록 처리한다.
            try
            {
                _agentRenderer = Enemy.GetCompo<AgentRenderer>();
                _attackExecutor = Enemy.GetCompo<AttackExecutor>();
                _skillManager = Object.FindFirstObjectByType<BattleSkillManager>();
            }
            catch (NullReferenceException e)
            {
                Debug.LogException(e);
                Debug.LogError($"{Enemy.name}의 AttackState가 설정될 수 없습니다.");
                Enemy.ChangeState(EnemyStates.IDLE);
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
                Enemy.ChangeState(EnemyStates.IDLE);
                yield break;
            }
            
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

            SkillCameraManager.Instance.SetAnchor(CamAnchor.Target, ctx.User.transform);
            //만약 캐릭터가 움직이는 상태였다면
            if (ctx.Stance == AttackStance.StepForward || ctx.Stance == AttackStance.DashToTarget)
            {
                //원래 자리로 돌아온다
                _moveTween?.Kill();
                _moveTween = Agent.transform.DOMove(_spawnPosition, 0.15f).SetEase(Ease.InSine);
                yield return _moveTween.WaitForCompletion();
                if (_isExiting) yield break;
            }

            yield return new WaitForSeconds(0.5f);
            
            //턴메니저에 보낼 스킬 종료 이벤트
            Bus<SkillFinishedEvent>.Raise(new SkillFinishedEvent(Agent, item));
            //공격이 모두 끝날 시 IDle로 전환
            Enemy.ChangeState(EnemyStates.IDLE);
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
            Debug.Log("전진 실행");
            var from = Agent.transform.position;
            var dir  = (ctx.CastPoint - from).normalized;
            var step = from + dir * ctx.ApproachOffset;
            
            //필요시 약간의 준비 지연을 트윈으로 처리
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

            Vector3 dest = ctx.CastPoint;
            float dirX = Mathf.Sign(dest.x - Agent.transform.position.x);
            
            if (Mathf.Approximately(dirX, 0f))
                dirX = 1f;

            dest.x -= dirX * ctx.ApproachOffset;
            
            return Agent.transform.DOMove(dest, 0.25f).SetEase(Ease.OutSine);
        }
    }
}