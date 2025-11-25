using System;
using _00.Work.Resource.Scripts.Managers;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Agents;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.FeedBacks;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.ItemTypes.ActiveItems;
using DG.Tweening;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Attacks.Skills
{
    public enum EffectSpawnAnchor
    {
        User,
        Target,
        CastPoint,
        UserToTarget
    }
    public class SkillEffectController : MonoBehaviour, ISkillHandler
    {
        public event Action OnComplete;

        [Header("Spawn Settings")]
        public EffectSpawnAnchor anchor = EffectSpawnAnchor.CastPoint;
        public Vector2 localOffset;
        public bool mirrorX;
        
        [Header("LifeTime Fallback")]
        public float fallbackDuration;
        
        [Header("Feedback")]
        [SerializeField] private FeedBackSystem feedBackSystem;
        [SerializeField] private bool isFeedbackActive;
        
        [Header("User Target Move")]
        [SerializeField] private float moveDuration = 0.4f;
        [SerializeField] private Ease moveEase = Ease.OutQuad;

        private Tween _moveTween;
        
        private SkillContent _ctx;
        private AttackItemSo _item;
        private bool _completed;
        private bool _impacted;

        private bool _arrivalDamageApplied;

        public void Init(SkillContent ctx, AttackItemSo item)
        {
            _ctx = ctx;
            _item = item;
            
            if (anchor == EffectSpawnAnchor.UserToTarget)
            {
                Vector2 offset = localOffset;

                // 좌우 반전
                if (mirrorX && ctx.User.TryGetComponent(out AgentRenderer ar) && ar.LookToTheLeft)
                {
                    offset.x = -offset.x;
                }

                // 시작 위치: User
                Vector3 start = ctx.User.transform.position;
                // 도착 위치: Target 또는 CastPoint
                Vector3 end = GetUserToTargetEndPosition(ctx);

                // 시작 위치 세팅
                transform.localPosition = start + (Vector3)offset;

                if (TryGetComponent(out AnimationEventRelay relay))
                {
                    relay.Handler = this;
                }

                // 카메라 앵커 설정
                SkillCameraManager.Instance.SetAnchor(CamAnchor.Target, transform);
                SkillCameraManager.Instance.ZoomTo(8f);
                
                Vector3 destination = end + (Vector3)offset;

                // User → Target 이동 + 도착 시 데미지
                _moveTween = transform
                    .DOMove(destination, moveDuration)
                    .SetEase(moveEase)
                    .OnComplete(() =>
                    {
                        _moveTween = null;
                        AnimApplyDamageNow(); // 도착 순간 데미지
                        NotifyEnd(); // 이펙트 종료
                    });

                return; // UserToTarget는 여기서 끝
            }
            
            Vector2 pos = GetAnchorPosition(ctx, anchor);
            Vector2 baseOffset = localOffset;
            
            if (mirrorX && ctx.User.TryGetComponent(out AgentRenderer agentRenderer) && agentRenderer.LookToTheLeft)
            {
                baseOffset .x = -baseOffset.x;
            }
            
            transform.localPosition = pos + baseOffset;

            if (TryGetComponent(out AnimationEventRelay relayDefault))
            {
                relayDefault.Handler = this;
            }

        }
        
        private void OnDestroy()
        {
            _moveTween?.Kill();
        }
        
        private Vector3 GetUserToTargetEndPosition(SkillContent ctx)
        {
            // 타겟이 여러 명이면 CastPoint(중앙)
            if (ctx.Targets is { Count: > 1 })
            {
                return ctx.CastPoint;
            }

            // 타겟이 1명만 있으면 그 타겟 위치
            if (ctx.Targets is { Count: 1 } && ctx.Targets[0] != null)
            {
                return ctx.Targets[0].transform.position;
            }

            // 타겟이 없으면 그냥 시전자 위치
            return ctx.User.transform.position;
        }
        private void Update()
        {
            if (anchor == EffectSpawnAnchor.UserToTarget)
                return;
            
            //애니메이션 이벤트가 없는 경우 사용
            if (!_completed && fallbackDuration > 0)
            {
                fallbackDuration -= Time.deltaTime;
                if (fallbackDuration <= 0) NotifyEnd();
            }
        }

        public void PlayFeedback()
        {
            if (!isFeedbackActive || feedBackSystem == null) return;
            
            feedBackSystem.PlayFeedback();
        }

        public void NotifyImpact() //애니메이션 시작을 알리는 함수
        {
            if (_impacted) return;
            _impacted = true;
        }

        public void AnimApplyDamageNow() //이벤트에서 데미지를 주는 함수
        {
            if (_item != null)
            {
                _item.ApplyDamageNow(_ctx);
            }
        }

        public void NotifyEnd() //애니메이션 종료를 알리는 함수
        {
            if (_completed) return;
            _completed = true;
            OnComplete?.Invoke();
            Destroy(gameObject);
        }

        //스폰 위치에 따라 고정 위치를 찾고, 카메라를 설정함
        
        private Vector3 GetAnchorPosition(SkillContent ctx, EffectSpawnAnchor spawnAnchor)
        {
            switch (spawnAnchor)
            {
                case EffectSpawnAnchor.User:
                    SkillCameraManager.Instance.SetAnchor(CamAnchor.Target, transform);
                    SkillCameraManager.Instance.ZoomTo(8f);
                    return ctx.User.transform.position;
                    
                case EffectSpawnAnchor.Target:
                    SkillCameraManager.Instance.SetAnchor(CamAnchor.Target, transform);
                    SkillCameraManager.Instance.ZoomTo(8f);

                    if (ctx.Targets is { Count: > 1 })
                    {
                        return ctx.CastPoint;
                    }

                    if (ctx.Targets is { Count: 1 } && ctx.Targets[0] != null)
                    {
                        return ctx.Targets[0].transform.position;
                    }

                    return ctx.User.transform.position;

                case EffectSpawnAnchor.CastPoint:
                    return ctx.CastPoint;
                case EffectSpawnAnchor.UserToTarget:
                default:
                    return ctx.User.transform.position;
            }
        }
    }
}