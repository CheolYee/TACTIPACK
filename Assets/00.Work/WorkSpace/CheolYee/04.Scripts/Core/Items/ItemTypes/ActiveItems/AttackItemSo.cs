
using _00.Work.WorkSpace.CheolYee._04.Scripts.Agents;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Attacks;
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

        public virtual ISkillHandler ActiveWithSkillContent(SkillContent ctx)
        {
            //스킬 프리팹 (이펙트) 가 있나?
            if (skillPrefab != null)
            {
                //있다면 생성 후 스킬 핸들러 반환
                var go = Instantiate(skillPrefab, ctx.CastPoint, Quaternion.identity);
                go.TryGetComponent(out ISkillHandler handler);
                
                return handler;
            }
            
            //데미지는 무조건 들어감
            ApplyDamageNow(ctx);
            //아니라면 널 반환
            return null;
        }

        //스킬 데이터를 돌며 데미지 주기
        private void ApplyDamageNow(SkillContent ctx)
        {
            //타겟모드가 싱글이고 0보다 타겟이 많다면
            if (ctx.TargetingMode == TargetingMode.Single && ctx.Targets.Count > 0)
            {
                //해당 타겟에게 데미지를 준다
                ctx.Targets[Index].ApplyDamage(DefaultAttackData);
            }
            else //아니라면
            {
                //모든 타겟을 돌며 AgentHEalth를 가져와 데미지를 준다
                foreach (AgentHealth target in ctx.Targets)
                {
                    target.ApplyDamage(DefaultAttackData);   
                }
            }
        }

        //스킬 프리팹을 생성해 이펙트 처리, ISkillHandler가 있다면 리턴 아니면 null 반환
        public virtual ISkillHandler ActiveAndGetSkillHandler(GameObject user, GameObject target = null)
        {
            if (skillPrefab == null || target == null) return null;

            Transform userTrm = user.transform;
            GameObject skillGo = Instantiate(skillPrefab, target.gameObject.transform.position, target.gameObject.transform.rotation);
            
            skillGo.TryGetComponent(out ISkillHandler handler);
            return handler;
        }
    }
}