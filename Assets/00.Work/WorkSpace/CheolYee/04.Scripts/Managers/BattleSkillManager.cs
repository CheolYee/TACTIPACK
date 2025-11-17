using System.Collections.Generic;
using System.Linq;
using _00.Work.Scripts.Managers;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Agents;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Attacks;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.ItemTypes.ActiveItems;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Creatures.Enemies;
using UnityEngine;
using UnityEngine.Serialization;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Managers
{
    public class BattleSkillManager : MonoSingleton<BattleSkillManager>
    {
        [SerializeField] private List<AgentHealth> playerTargets = new();
        [SerializeField] private List<AgentHealth> enemyTargets = new();

        //플레이어가 에너미를 타겟팅할 때 쓸 메서드
        public void SetPlayerTargets(IEnumerable<AgentHealth> targets)
        {
            playerTargets.Clear();
            playerTargets.AddRange(targets.Where(t => t != null));
        }
        
        //에너미가 플레이어를 타겟팅할 때 쓸 메서드
        public void SetEnemyTargets(IEnumerable<AgentHealth> targets)
        {
            enemyTargets.Clear();
            enemyTargets.AddRange(targets.Where(t => t != null));
        }
        
        private List<AgentHealth> GetTargetsForUser(Agent user)
        {
            if (user is Enemy)
                return GetPlayerTargets();

            // 기본은 플레이어용으로 가정
            return GetEnemyTargets();
        }
        
        public List<AgentHealth> GetPlayerTargets() => playerTargets.Where(t => t != null).ToList();
        public List<AgentHealth> GetEnemyTargets() => enemyTargets.Where(t => t != null).ToList();

        //스킬의 생성 위치를 설정합니다.
        public Vector3 ResolveCastPoint(Agent user, TargetingMode mode, int idx = 0)
        {
            List<AgentHealth> list = GetTargetsForUser(user);
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
                case TargetingMode.None:
                    return user.transform.position;
                default:
                    return user.transform.position;
            }
        }

        public SkillContent BuildContext(Agent user, AttackItemSo item, AttackStance? overrideStance = null)
        {
            List<AgentHealth> targets = GetTargetsForUser(user);
            return new SkillContent()
            {
                User = user,
                Targets = targets,
                CastPoint = ResolveCastPoint(user, item.TargetingMode, item.TargetIndex),
                TargetingMode = item.TargetingMode,
                Stance = overrideStance ?? AttackStance.Stationary,
                ApproachOffset = item.ApproachOffset
            };
        }
    }
}