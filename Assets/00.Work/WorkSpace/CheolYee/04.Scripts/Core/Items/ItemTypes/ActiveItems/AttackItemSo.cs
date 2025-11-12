
using _00.Work.WorkSpace.CheolYee._04.Scripts.Agents;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Attacks;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Attacks.Skills;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.ItemTypes.ActiveItems
{
    [CreateAssetMenu(fileName = "newAttackItem", menuName = "SO/Item/ActiveItem/AttackItem", order = 0)]
    public class AttackItemSo : ActiveItemDataSo
    {
        [Header("Attack")]
        [field: SerializeField] public AttackDataSo DefaultAttackData { get; private set; }
        [field: SerializeField] public int Index { get; private set; }
        [field: SerializeField] public float SkillDelay { get; private set; }
        
        [Header("Flow Policy")]
        [field: SerializeField] public TargetingMode TargetingMode { get; private set; }
        [field: SerializeField] public AttackStance DefaultStance { get; private set; } 
        [field: SerializeField] public float ApproachOffset { get; private set; }


        [Header("Damage Timing")] [SerializeField]
        private bool damageOnImpactEvent = true; //허용 시 애니메이션 이벤트에서 데미지 처리
        //비허용이라면 스폰 즉시 들어감
        public virtual ISkillHandler ActiveWithSkillContent(SkillContent ctx)
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
            
            //아니라면 널 반환 후 데미지 즉시처리
            ApplyDamageNow(ctx);
            return null;
        }

        //스킬 데이터를 돌며 데미지 주기
        public void ApplyDamageNow(SkillContent ctx)
        {
            //타겟모드가 싱글이고 0보다 타겟이 많다면
            switch (ctx.TargetingMode)
            {
                case TargetingMode.Single:
                    //싱글일때에는 한놈만 받아 처리하므로 0번쨰 인덱스를 가져옴
                    if (ctx.Targets.Count > 0)
                        ctx.Targets[0]
                        .ApplyDamage(DefaultAttackData);
                    break;
                case TargetingMode.Random:
                    if (ctx.Targets.Count > 0)
                    {
                        //타겟이 1명이라도 있으면 렌덤을 돌려 한명에게 데미지를 준다
                        int randomIndex = Random.Range(0, ctx.Targets.Count);
                        ctx.Targets[randomIndex].ApplyDamage(DefaultAttackData);
                    }
                    break;
                
                //범위라면 모든 타겟을 순회하며 데미지를 준다
                case TargetingMode.Area:
                    foreach (AgentHealth target in ctx.Targets)
                    {
                        target.ApplyDamage(DefaultAttackData);
                    }
                    break;
            }
        }
    }
}