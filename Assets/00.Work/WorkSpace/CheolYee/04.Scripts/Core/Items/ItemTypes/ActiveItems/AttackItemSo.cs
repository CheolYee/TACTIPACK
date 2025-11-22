
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
    public enum SkillTargetGroup
    {
        Targets = 0,     //지금 스킬이 잡고 있는 타겟
        Self,            //자신
        AlliesAll,       //아군 전체
        AlliesRandomOne, //랜덤 아군 1인
        EnemiesAll,      //적 전체
        EnemiesRandomOne, //랜덤 적 1인
        AlliesByIndex, // 아군 인덱스로 1명
        EnemiesByIndex, // 적 인덱스로 1명
        NextAllyInOrder //플레이어 전용 다음 턴의 캐릭터
    }
    
    [Serializable]
    public struct StatusCleanseInfo
    {
        public StatusEffectType type; //어떤 디버프를 깎을지 (None이면 모든 디버프)
        public SkillTargetGroup targetGroup; //자신, 아군 전체, 타겟들 등
        public int durationTurns; //몇 턴 깎을지
        public int percentage; //적용 확률(0~100)
    }
    
    [Serializable]
    public struct StatusEffectInfo
    {
        public StatusEffectType type; //타입
        public SkillTargetGroup targetGroup; //어떤 대상에게 상태이상 거는지
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
        [field: SerializeField] public float SkillDelay { get; private set; }
        
        [Header("Main Target")]
        [SerializeField] private SkillTargetGroup mainTargetGroup = SkillTargetGroup.EnemiesAll;
        [SerializeField] private int mainTargetIndex;
        
        public SkillTargetGroup MainTargetGroup => mainTargetGroup;
        public int MainTargetIndex => mainTargetIndex;
        
        [Header("Flow Policy")]
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
        [Header("Cleanse Effects")]
        [field: SerializeField] public StatusCleanseInfo[] CleanseEffects { get; private set; }
        
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

        //스킬 데이터를 돌며 스킬에 맞는 효과 실행
        public void ApplyDamageNow(SkillContent ctx)
        {
            if (ctx == null) return;

            Agent attacker = ctx.User;
            if (attacker == null) return;

            var mgr = BattleSkillManager.Instance;
            if (mgr == null) return;

            StatusEffectController attackerStatus = attacker.StatusEffectController;
            
            var mainTargets = ctx.Targets;
            if (mainTargets == null || mainTargets.Count == 0)
                return;


            // 1. 힐 스킬 처리
            if (isHealSkill)
            {
                //BuildContext 에서 이미 확정된 mainTargets 사용
                foreach (var target in mainTargets)
                {
                    if (target == null) continue;

                    float healAmount;

                    if (useMaxHpRatioHeal)
                    {
                        float ratio = maxHpHealRatio;
                        healAmount = target.MaxHealth * ratio;
                    }
                    else
                    {
                        healAmount = DefaultAttackData != null ? DefaultAttackData.Damage : 0f;
                    }

                    target.Heal(healAmount);
                }

                //디버프 스테이터스 적용 처리
                ApplyStatusEffects(attacker, ctx, stunProc: false);
                ApplyCleanseEffects(attacker, ctx);
                return;
            }


            // 2. 데미지 스킬 처리
            
            // 데미지 계산
            float baseDamage = DefaultAttackData != null ? DefaultAttackData.Damage : 0f;
            float attackRate = attackerStatus != null ? attackerStatus.GetAttackRate() : 1f;
            float scaledDamage = baseDamage * attackRate;

            bool isCritical = false;
            if (canCritical)
            {
                float baseCrit  = attacker.GetBaseCritChance();
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

            // 스턴 한 번만 굴리기
            bool stunProc = false;
            int stunRoll = Random.Range(0, 100); // 0~99

            if (DefaultStatusEffect != null)
            {
                foreach (var status in DefaultStatusEffect)
                {
                    if (status.type != StatusEffectType.Stun) continue;

                    if (stunRoll < status.percentage)
                    {
                        stunProc = true;
                        break;
                    }
                }
            }

            _damageContainer = new DamageContainer(scaledDamage, isCritical);
            
            foreach (var target in mainTargets)
            {
                if (target == null) continue;
                target.ApplyDamage(_damageContainer);
            }

            // 상태이상 / 디버프 제거
            ApplyStatusEffects(attacker, ctx, stunProc);
            ApplyCleanseEffects(attacker, ctx);
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

                //스턴은 미리 굴린 결과가 false면 스킵
                if (info.type == StatusEffectType.Stun && !stunProc)
                    continue;

                //나머지 상태이상 확률
                if (info.type != StatusEffectType.Stun &&
                    info.percentage is > 0 and < 100)
                {
                    int roll = Random.Range(0, 100);
                    if (roll >= info.percentage)
                        continue;
                }
                
                List<AgentHealth> targets = mgr.ResolveTargets(info.targetGroup, attacker, ctx);
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
        
        private void ApplyCleanseEffects(Agent attacker, SkillContent ctx)
        {
            if (CleanseEffects == null || CleanseEffects.Length == 0) return;
            if (attacker == null) return;

            BattleSkillManager mgr = BattleSkillManager.Instance;
            if (mgr == null) return;

            foreach (var info in CleanseEffects)
            {
                if (info.durationTurns <= 0)
                    continue;

                //확률 체크
                if (info.percentage is > 0 and < 100)
                {
                    int roll = Random.Range(0, 100);
                    if (roll >= info.percentage)
                        continue;
                }

                //공통 타겟 함수 사용
                List<AgentHealth> targets = mgr.ResolveTargets(info.targetGroup, attacker, ctx);
                if (targets == null || targets.Count == 0)
                    continue;

                foreach (var t in targets)
                {
                    if (t == null || t.Owner == null) continue;

                    var controller = t.Owner.GetCompo<StatusEffectController>();
                    controller?.ReduceDebuffs(info.type, info.durationTurns);
                }
            }
        }
    }
}