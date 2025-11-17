using System;
using _00.Work.Resource.Scripts.Managers;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Agents;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.ItemTypes.ActiveItems;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Attacks.Skills
{
    public enum EffectSpawnAnchor
    {
        User,
        Target,
        CastPoint
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

        private SkillContent _ctx;
        private AttackItemSo _item;
        private bool _completed;
        private bool _impacted;


        public void Init(SkillContent ctx, AttackItemSo item)
        {
            _ctx = ctx;
            _item = item;
            
            Vector2 pos = GetAnchorPosition(ctx, anchor);
            Vector2 offset = localOffset;
            
            if (mirrorX && ctx.User.TryGetComponent(out AgentRenderer ar) && ar.LookToTheLeft)
            {
                offset.x = -offset.x;
            }
            
            transform.localPosition = pos + offset;

            if (TryGetComponent(out AnimationEventRelay relay))
            {
                relay.Handler = this;
            }

        }
        private void Update()
        {
            //애니메이션 이벤트가 없는 경우 사용
            if (!_completed && fallbackDuration > 0)
            {
                fallbackDuration -= Time.deltaTime;
                if (fallbackDuration <= 0) NotifyEnd();
            }
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
                    SkillCameraManager.Instance.ZoomTo(7f);
                    return ctx.User.transform.position;
                case EffectSpawnAnchor.Target:
                    SkillCameraManager.Instance.SetAnchor(CamAnchor.Target, transform);
                    SkillCameraManager.Instance.ZoomTo(7f);
                    return (ctx.Targets != null && ctx.Targets.Count > 0) 
                        ? ctx.Targets[0].transform.position 
                        : ctx.User.transform.position;
                case EffectSpawnAnchor.CastPoint:
                default:
                    return ctx.CastPoint;
            }
        }
    }
}