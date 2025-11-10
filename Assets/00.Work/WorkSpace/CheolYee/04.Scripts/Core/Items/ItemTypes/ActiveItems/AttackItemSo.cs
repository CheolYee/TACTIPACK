
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
            if (skillPrefab != null)
            {
                var go = Object.Instantiate(skillPrefab, ctx.CastPoint, Quaternion.identity);
                go.TryGetComponent(out ISkillHandler handler);
                
                return handler;
            }

            ApplyDamageNow(ctx);
            return null;
        }

        private void ApplyDamageNow(SkillContent ctx)
        {
            if (ctx.TargetingMode == TargetingMode.Single && ctx.Targets.Count > 0)
            {
                ctx.Targets[Index].ApplyDamage(DefaultAttackData);
            }
            else
            {
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