
using System;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Agents;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Attacks;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Attacks.Damages;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Attacks.Skills;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.ItemTypes.ActiveItems
{
    [Serializable]
    public struct StatusEffectInfo
    {
        public StatusEffectType type; //타입
        public int durationTurns; //유지 턴 수
        public float power; //세기
        public bool stackable; //중첩 가능한지?
    }
    
    [CreateAssetMenu(fileName = "newAttackItem", menuName = "SO/Item/ActiveItem/AttackItem", order = 0)]
    public class AttackItemSo : ActiveItemDataSo, IAttacker
    {
        [Header("Attack")]
        [field: SerializeField] public AttackDataSo DefaultAttackData { get; private set; }
        [field: SerializeField] public int TargetIndex { get; private set; }
        [field: SerializeField] public float SkillDelay { get; private set; }
        
        [Header("Flow Policy")]
        [field: SerializeField] public TargetingMode TargetingMode { get; private set; }
        [field: SerializeField] public AttackStance DefaultStance { get; private set; } 
        [field: SerializeField] public float ApproachOffset { get; private set; }


        [Header("Damage Timing")] 
        [SerializeField] private bool damageOnImpactEvent; //허용 시 애니메이션 이벤트에서 데미지 처리
        
        //스킬이 거는 상태이상
        [Header("Status Effects")]
        [field :SerializeField] public StatusEffectInfo[] DefaultStatusEffect { get; private set; } 
        
        private DamageContainer _damageContainer;

        //비허용이라면 스폰 즉시 들어감
        public ISkillHandler ActiveWithSkillContent(SkillContent ctx)
        {
            //스킬 프리팹 (이펙트) 가 있나?
            if (skillPrefab != null)
            {
                //있다면 생성 후 스킬 핸들러 반환
                GameObject go = Instantiate(skillPrefab, ctx.CastPoint, Quaternion.identity);
                if (!go.TryGetComponent(out ISkillHandler handler)) //스킬 핸들러가 없다면 자동 생성
                {
                    ISkillHandler skillHandler = go.AddComponent<SkillEffectController>();
                    handler = skillHandler;
                }
                
                handler.Init(ctx, this);
                
                if (!damageOnImpactEvent) //이벤트 사용 유무 판단
                    ApplyDamageNow(ctx);
                
                return handler;
            }
            
            return null;
        }

        //스킬 데이터를 돌며 데미지 주기
        public void ApplyDamageNow(SkillContent ctx)
        {
            _damageContainer = new DamageContainer(DefaultAttackData.Damage);
            //타겟모드가 싱글이고 0보다 타겟이 많다면
            switch (ctx.TargetingMode)
            {
                case TargetingMode.Single:
                    //싱글일때에는 한놈만 받아 처리하므로 0번쨰 인덱스를 가져옴
                    if (ctx.Targets.Count > 0)
                    {
                        AgentHealth target = ctx.Targets[0];
                        ctx.Targets[0].ApplyDamage(_damageContainer);
                        ApplyStatusEffectsToTarget(ctx.User, target);
                    }
                    break;
                case TargetingMode.Random:
                    if (ctx.Targets.Count > 0)
                    {
                        //타겟이 1명이라도 있으면 렌덤을 돌려 한명에게 데미지를 준다
                        int randomIndex = Random.Range(0, ctx.Targets.Count);
                        AgentHealth target = ctx.Targets[randomIndex];
                        target.ApplyDamage(_damageContainer);
                        ApplyStatusEffectsToTarget(ctx.User, target);
                    }
                    break;
                
                //범위라면 모든 타겟을 순회하며 데미지를 준다
                case TargetingMode.Area:
                    foreach (AgentHealth target in ctx.Targets)
                    {
                        target.ApplyDamage(_damageContainer);
                        ApplyStatusEffectsToTarget(ctx.User, target);
                    }
                    break;
            }
        }
        
        //상태이상 적용
        private void ApplyStatusEffectsToTarget(Agent attacker, AgentHealth targetHealth)
        {
            if (DefaultStatusEffect == null || DefaultStatusEffect.Length == 0) return;
            if (targetHealth == null || targetHealth.Owner == null) return;

            var controller = targetHealth.Owner.GetCompo<StatusEffectController>();
            if (controller == null) return;

            foreach (var info in DefaultStatusEffect)
            {
                controller.AddStatus(info);
            }
        }
    }
}