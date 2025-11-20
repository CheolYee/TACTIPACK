
using System;
using System.Collections.Generic;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Agents;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Attacks;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Attacks.Damages;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Attacks.Skills;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Managers;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.ItemTypes.ActiveItems
{
    public enum StatusTargetGroup
    {
        Targets = 0,     //지금 스킬이 잡고 있는 타겟
        Self,            //자신
        AlliesAll,       //아군 전체
        AlliesRandomOne, //랜덤 아군 1인
        EnemiesAll,      //적 전체
        EnemiesRandomOne //랜덤 적 1인
    }
    
    [Serializable]
    public struct StatusEffectInfo
    {
        public StatusEffectType type; //타입
        public StatusTargetGroup targetGroup; //어떤 대상에게 상태이상 거는지
        public int durationTurns; //유지 턴 수
        public float power; //세기
        public int percentage; //확률
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
        
        [Header("Heal")]
        [SerializeField] private bool isHealSkill;
        [SerializeField] private bool useMaxHpRatioHeal;
        [SerializeField] [Range(0f, 1f)] private float maxHpHealRatio = 0.3f;
        public bool IsHealSkill => isHealSkill;

        [Header("Damage Timing")] 
        [SerializeField] private bool damageOnImpactEvent; //허용 시 애니메이션 이벤트에서 데미지 처리
        
        [Header("Critical")]
        [SerializeField] private bool canCritical = true;
        private const float CriticalMultiplier = 1.5f; //크리 배수
        
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
            if (ctx == null || ctx.Targets == null || ctx.Targets.Count == 0)
                return;
            
            Agent attacker = ctx.User;
            StatusEffectController attackerStatus = attacker != null ? attacker.StatusEffectController : null;
            
            if (isHealSkill)
            {
                foreach (var target in ctx.Targets)
                {
                    if (target == null) continue;

                    float healAmount = 0f;

                    if (useMaxHpRatioHeal)
                    {
                        //최대체력 비례 힐
                        float ratio = maxHpHealRatio;
                        healAmount = target.MaxHealth * ratio;
                    }
                    else
                    {
                        //기본(공격력 = 회복량)
                        healAmount = DefaultAttackData != null ? DefaultAttackData.Damage : 0f;
                    }

                    target.Heal(healAmount);
                }
                return; //힐은 여기서 끝
            }

            //공격력 벞디벞 반영
            float baseDamage = DefaultAttackData != null ? DefaultAttackData.Damage : 0f;
            float attackRate = attackerStatus != null ? attackerStatus.GetAttackRate() : 1f;
            float scaledDamage = baseDamage * attackRate;
            
            bool isCritical = false;
            if (canCritical)
            {
                float baseCrit = attacker != null ? attacker.GetBaseCritChance() : 0f;
                float extraCrit = attackerStatus != null ? attackerStatus.GetCritChanceModifier() : 0f;
                float totalCrit = Mathf.Clamp01(baseCrit + extraCrit);

                if (totalCrit > 0f)
                {
                    float rollCrit = Random.Range(0f, 1f);
                    if (rollCrit < totalCrit)
                    {
                        isCritical = true;
                        scaledDamage *= CriticalMultiplier;
                    }
                }
            }
            
            bool stunProc = false;
            int stunRoll = Random.Range(0, 100); // 0 ~ 99

            if (DefaultStatusEffect != null)
            {
                foreach (var status in DefaultStatusEffect)
                {
                    if (status.type != StatusEffectType.Stun)
                        continue;
                    
                    if (stunRoll < status.percentage)
                    {
                        stunProc = true;
                        break;
                    }
                }
            }
            
            _damageContainer = new DamageContainer(scaledDamage, isCritical);

            //타겟에게 데미지 적용
            foreach (AgentHealth target in ctx.Targets)
            {
                if (target == null) continue;
                target.ApplyDamage(_damageContainer);
            }

            //상태이상(버프/디버프/스턴/베리어) 적용
            ApplyStatusEffects(attacker, ctx, stunProc);
        }
        
        private void ApplyStatusEffects(Agent attacker, SkillContent ctx, bool stunProc)
        {
            if (DefaultStatusEffect == null || DefaultStatusEffect.Length == 0) return;
            if (attacker == null) return;

            BattleSkillManager mgr = BattleSkillManager.Instance;
            if (mgr == null) return;

            foreach (var info in DefaultStatusEffect)
            {
                if (info.type == StatusEffectType.None)
                    continue;

                // 스턴은 미리 굴린 결과가 false면 아예 스킵
                if (info.type == StatusEffectType.Stun && !stunProc)
                    continue;

                // Stun 외에도 확률을 쓰고 싶다면: percentage가 0~100 사이일 때만 처리
                if (info.type != StatusEffectType.Stun &&
                    info.percentage is > 0 and < 100)
                {
                    int roll = Random.Range(0, 100);
                    if (roll >= info.percentage)
                        continue;
                }

                List<AgentHealth> targets = ResolveStatusTargets(attacker, ctx, info, mgr);
                if (targets == null || targets.Count == 0)
                    continue;

                foreach (AgentHealth t in targets)
                {
                    if (t == null || t.Owner == null) continue;
                    var controller = t.Owner.GetCompo<StatusEffectController>();
                    controller?.AddStatus(info);
                }
            }
        }

        private List<AgentHealth> ResolveStatusTargets(
            Agent attacker,
            SkillContent ctx,
            StatusEffectInfo info,
            BattleSkillManager mgr)
        {
            switch (info.targetGroup)
            {
                case StatusTargetGroup.Targets:
                    //지금 스킬이 타겟으로 잡은 애들
                    return ctx.Targets;

                case StatusTargetGroup.Self:
                    if (attacker.Health != null)
                        return new List<AgentHealth> { attacker.Health };
                    break;

                case StatusTargetGroup.AlliesAll:
                    return mgr.GetAlliesOf(attacker);

                case StatusTargetGroup.AlliesRandomOne:
                {
                    var allies = mgr.GetAlliesOf(attacker);
                    var picked = mgr.PickRandomOne(allies);
                    if (picked != null)
                        return new List<AgentHealth> { picked };
                    break;
                }

                case StatusTargetGroup.EnemiesAll:
                    return mgr.GetOpponentsOf(attacker);

                case StatusTargetGroup.EnemiesRandomOne:
                {
                    var enemies = mgr.GetOpponentsOf(attacker);
                    var picked = mgr.PickRandomOne(enemies);
                    if (picked != null)
                        return new List<AgentHealth> { picked };
                    break;
                }
            }

            return null;
        }
    }
}