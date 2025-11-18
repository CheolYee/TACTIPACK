using System.Collections.Generic;
using System.Linq;
using _00.Work.Scripts.Managers;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Agents;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Attacks;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Events;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.ItemTypes.ActiveItems;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Creatures.Enemies;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Creatures.Players;
using _00.Work.WorkSpace.CheolYee._04.Scripts.UI.HealthBar;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Managers
{
    public class BattleSkillManager : MonoSingleton<BattleSkillManager>
    {
        [SerializeField] private List<AgentHealth> players = new();
        [SerializeField] private List<AgentHealth> enemies = new();

        private bool _battleEnded;
        
        public void RegisterPlayer(AgentHealth health)
        {
            if (health == null) return;
            if (!players.Contains(health))
                players.Add(health);
        }
        
        public void RegisterEnemy(AgentHealth health)
        {
            if (health == null) return;
            if (!enemies.Contains(health))
                enemies.Add(health);
        }
        
        public void Unregister(AgentHealth health)
        {
            if (health == null) return;
            players.Remove(health);
            enemies.Remove(health);

            CheckBattleEnd();
        }

        private void CheckBattleEnd()
        {
            if (_battleEnded) return;

            bool anyPlayerAlive = players.Any(h => h != null);
            bool anyEnemyAlive = enemies.Any(h => h != null);

            if (!anyEnemyAlive && anyPlayerAlive)
            {
                _battleEnded = true;
                Debug.Log("[BattleSkillManager] 모든 에너미가 사망했습니다. Victory!");
                Bus<BattleResultEvent>.Raise(new BattleResultEvent(BattleResultType.Victory));
                return;
            }

            if (!anyPlayerAlive && anyEnemyAlive)
            {
                _battleEnded = true;
                Debug.Log("[BattleSkillManager] 모든 플레이어가 사망했습니다. Defeat...");
                Bus<BattleResultEvent>.Raise(new BattleResultEvent(BattleResultType.Defeat));
                return;
            }
        }

        private List<AgentHealth> GetTargetsForUser(Agent user)
        {
            if (user is Enemy)
                return GetPlayerTargets();

            //기본은 플레이어용으로 가정
            return GetEnemyTargets();
        }

        public List<Player> GetPlayers()
        {
            List<Player> playerCompos = new();

            foreach (var p in players)
            {
                playerCompos.Add(p.Owner as Player);
            }
            
            return playerCompos;
        }
        
        public List<Enemy> GetEnemies()
        {
            List<Enemy> enemyCompos = new();

            foreach (var e in this.enemies)
            {
                enemyCompos.Add(e.Owner as Enemy);
            }
            
            return enemyCompos;
        }
        
        public List<AgentHealth> GetPlayerTargets() => players.Where(t => t != null).ToList();
        public List<AgentHealth> GetEnemyTargets() => enemies.Where(t => t != null).ToList();

        //스킬의 생성 위치를 설정합니다.
        public Vector3 ResolveCastPoint(Agent user, TargetingMode mode, int idx = 0)
        {
            List<AgentHealth> list = GetTargetsForUser(user);
            
            if (list.Count == 0) return user.transform.position; //만약 타겟이 없으면 그냥 리턴

            switch (mode)
            {
                case TargetingMode.Single: //단일
                    HudManager.Instance.ShowOnly(list[idx].HealthBarInstance);
                    return list[idx].transform.position; //인덱스 (선택 안하면 첫번째거 리턴)
                
                case TargetingMode.Area: //범위
                    List<HealthBarUi> barList = new();
                    list.ForEach(h => barList.Add(h.HealthBarInstance));
                    HudManager.Instance.ShowOnly(barList);
                    Vector3 sum = Vector3.zero;
                    foreach (var t in list) sum += t.transform.position;
                    return sum / list.Count; //모든 적의 위치의 중앙값
                
                case TargetingMode.Random: //랜덤
                    int randomIndex = Random.Range(0, list.Count);
                    HudManager.Instance.ShowOnly(list[randomIndex].HealthBarInstance);
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