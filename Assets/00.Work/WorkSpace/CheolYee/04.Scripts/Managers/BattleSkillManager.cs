using System.Collections.Generic;
using System.Linq;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Agents;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Attacks;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.ItemTypes.ActiveItems;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Managers
{
    public class BattleSkillManager : MonoBehaviour
    {
        private readonly List<AgentHealth> _targets = new();

        //시전 시 모든 타겟을 지정할 메서드
        public void SetTargets(IEnumerable<AgentHealth> targets)
        {
            _targets.Clear();
            _targets.AddRange(targets.Where(t => t != null));
        }
        
        public List<AgentHealth> GetTargets() => _targets.Where(t => t != null).ToList();

        //스킬의 생성 위치를 설정합니다.
        public Vector3 ResolveCastPoint(Agent user, TargetingMode mode, int idx = 0)
        {
            List<AgentHealth> list = GetTargets();
            if (list.Count == 0) return user.transform.position; //만약 타겟이 없으면 그냥 리턴

            switch (mode)
            {
                case TargetingMode.Single: //단일
                    return list[idx].transform.position; //인덱스 (선택 안하면 첫번째거 리턴)
                case TargetingMode.Area: //범위
                    Vector3 sum = Vector3.zero;
                    foreach (var t in list) sum += t.transform.position;
                    return sum / list.Count; //모든 적의 위치의 중앙값
                case TargetingMode.Random: //랜덤
                    int randomIndex = Random.Range(0, list.Count);
                    return list[randomIndex].transform.position;
                default:
                    return user.transform.position;
            }
        }

        public SkillContent BuildContext(Agent user, AttackItemSo item, AttackStance? overrideStance = null)
        {
            List<AgentHealth> targets = GetTargets();
            return new SkillContent()
            {
                User = user,
                Targets = targets,
                CastPoint = ResolveCastPoint(user, item.TargetingMode, item.Index),
                TargetingMode = item.TargetingMode,
                Stance = overrideStance ?? AttackStance.Stationary,
                ApproachOffset = item.ApproachOffset
            };
        }
    }
}