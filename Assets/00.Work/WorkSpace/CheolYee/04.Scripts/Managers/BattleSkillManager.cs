using System.Collections.Generic;
using System.Linq;
using _00.Work.Scripts.Managers;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Agents;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Attacks;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Attacks.Damages;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Events;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.ItemTypes.ActiveItems;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Creatures.Enemies;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Creatures.Players;
using _00.Work.WorkSpace.CheolYee._04.Scripts.UI.HealthBar;
using _00.Work.WorkSpace.CheolYee._04.Scripts.UI.Turn;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Managers
{
    public class BattleSkillManager : MonoSingleton<BattleSkillManager>
    {
        [SerializeField] private List<AgentHealth> players = new();
        [SerializeField] private List<AgentHealth> enemy = new();

        private bool _battleEnded;
        private BattleResultType _pendingResult = BattleResultType.None;
        
        public void RegisterPlayer(AgentHealth health)
        {
            if (health == null) return;
            if (!players.Contains(health))
                players.Add(health);
        }
        
        public void RegisterEnemy(AgentHealth health)
        {
            if (health == null) return;
            if (!enemy.Contains(health))
                enemy.Add(health);
        }
        
        public void Unregister(AgentHealth health)
        {
            if (health == null) return;
            players.Remove(health);
            enemy.Remove(health);

            CheckBattleEnd();
        }

        private void CheckBattleEnd()
        {
            if (_battleEnded) return;

            bool anyPlayerAlive = players.Any(h => h != null);
            bool anyEnemyAlive = enemy.Any(h => h != null);

            if (!anyEnemyAlive && anyPlayerAlive)
            {
                _battleEnded = true;
                _pendingResult = BattleResultType.Victory;
                return;
            }

            if (!anyPlayerAlive && anyEnemyAlive)
            {
                _battleEnded = true;
                _pendingResult = BattleResultType.Defeat;
            }
        }
        
        public bool TryConsumeBattleResult(out BattleResultType result)
        {
            result = BattleResultType.None;

            if (!_battleEnded || _pendingResult == BattleResultType.None)
                return false;

            result = _pendingResult;
            
            _pendingResult = BattleResultType.None;
            return true;
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
        
        public List<AgentHealth> GetPlayerTargets() => players.Where(IsAlive).ToList();
        public List<AgentHealth> GetEnemyTargets() => enemy.Where(IsAlive).ToList();
        
        public List<AgentHealth> GetAlliesOf(Agent user)
        {
            if (user is Enemy)
                return GetEnemyTargets();
            return GetPlayerTargets();
        }

        //user의 적 목록
        public List<AgentHealth> GetOpponentsOf(Agent user)
        {
            return GetTargetsForUser(user);
        }

        //리스트에서 랜덤으로 하나 뽑기
        public AgentHealth PickRandomOne(List<AgentHealth> list)
        {
            if (list == null) return null;
            list = list.Where(IsAlive).ToList();
            if (list.Count == 0) return null;

            int idx = Random.Range(0, list.Count);
            return list[idx];
        }
        
        private bool IsAlive(AgentHealth h)
        {
            if (h == null) return false;
            if (h.Owner == null) return false;
            if (h.Owner.IsDead) return false;
            if (h.CurrentHealth <= 0f) return false;
            return true;
        }

        public SkillContent BuildContext(Agent user, AttackItemSo item, AttackStance? overrideStance = null)
        {
            var ctx = new SkillContent
            {
                User = user,
                Stance = overrideStance ?? (item != null ? item.DefaultStance : AttackStance.Stationary),
                ApproachOffset = item != null ? item.ApproachOffset : 0f,
                Targets = new List<AgentHealth>()
            };

            if (user == null || item == null)
            {
                ctx.CastPoint = user != null ? user.transform.position : Vector3.zero;
                return ctx;
            }

            //메인 타겟 계산
            var mainTargets = ResolveTargets(item.MainTargetGroup, user, ctx, item.MainTargetIndex);
            ctx.Targets.AddRange(mainTargets);

            if (ctx.Targets.Count > 0)
            {
                ctx.CastPoint = GetCenterPoint(ctx.Targets);

                //HP바 표시도 동일 타겟 기준으로
                var bars = new List<HealthBarUi>();
                foreach (var h in ctx.Targets)
                {
                    if (h != null && h.HealthBarInstance != null)
                        bars.Add(h.HealthBarInstance);
                }

                if (bars.Count > 0)
                    HudManager.Instance.ShowOnly(bars);
            }
            else
            {
                //타겟이 없으면 자기 자신 기준
                ctx.CastPoint = user.transform.position;
                if (user.Health != null)
                    HudManager.Instance.ShowOnly(user.Health.HealthBarInstance);
            }

            return ctx;
        }
        
        private Vector3 GetCenterPoint(List<AgentHealth> list)
        {
            var sum = Vector3.zero;
            foreach (var h in list)
                sum += h.transform.position;

            return sum / list.Count;
        }
        
        public List<AgentHealth> ResolveTargets(
            SkillTargetGroup group,
            Agent user,
            SkillContent ctx,
            int index = 0)
        {
            var allies   = GetAlliesOf(user);
            var enemies  = GetOpponentsOf(user);

            switch (group)
            {
                case SkillTargetGroup.Targets:
                    return ctx.Targets;

                case SkillTargetGroup.Self:
                    return (user.Health != null) 
                        ? new List<AgentHealth> { user.Health }
                        : new List<AgentHealth>();

                case SkillTargetGroup.AlliesAll:
                    return allies;

                case SkillTargetGroup.EnemiesAll:
                    return enemies;

                case SkillTargetGroup.AlliesRandomOne:
                {
                    var picked = PickRandomOne(allies);
                    return picked != null ? new List<AgentHealth> { picked } : new List<AgentHealth>();
                }

                case SkillTargetGroup.EnemiesRandomOne:
                {
                    var picked = PickRandomOne(enemies);
                    return picked != null ? new List<AgentHealth> { picked } : new List<AgentHealth>();
                }

                case SkillTargetGroup.AlliesByIndex:
                    allies = allies.Where(IsAlive).ToList();
                    return (index >= 0 && index < allies.Count)
                        ? new List<AgentHealth> { allies[index] }
                        : new List<AgentHealth>();

                case SkillTargetGroup.EnemiesByIndex:
                    enemies = enemies.Where(IsAlive).ToList();
                    return (index >= 0 && index < enemies.Count)
                        ? new List<AgentHealth> { enemies[index] }
                        : new List<AgentHealth>();
                
                case SkillTargetGroup.NextAllyInOrder:
                {
                    var next = GetNextAllyInOrder(user, allies);
                    return (next != null) 
                        ? new List<AgentHealth> { next } 
                        : new List<AgentHealth>();
                }

                default:
                    return new List<AgentHealth>();
            }
        }
        
        public void ResetBattleState()
        {
            _battleEnded = false;

            // 혹시 이전 전투에서 null 이 남아있을 수 있으니 한 번 정리
            players = players.Where(h => h != null).ToList();
            enemy   = enemy.Where(h => h != null).ToList();
        }

        private AgentHealth GetNextAllyInOrder(Agent user, List<AgentHealth> allies)
        {
            if (user == null) return null;

            // 1) 플레이어라면 Turn UI 순서 먼저 시도
            if (user is Player player)
            {
                var panel = TurnUiContainerPanel.Instance;
                if (panel != null)
                {
                    var nextFromPanel = panel.GetNextPlayerHealthAfter(player);
                    if (nextFromPanel != null)
                        return nextFromPanel;
                }
            }

            // 2) fallback: allies 리스트에서 자기 Health 기준으로 다음 인덱스
            if (allies == null || allies.Count == 0 || user.Health == null)
                return null;

            // null 정리
            allies = allies.Where(h => h != null).ToList();
            if (allies.Count == 0)
                return null;

            int myIndex = allies.IndexOf(user.Health);
            if (myIndex < 0)
                return allies[0]; // 목록 안에 없으면 그냥 첫 아군 반환

            int nextIndex = (myIndex + 1) % allies.Count;
            return allies[nextIndex];
        }
        
        public void ClearAllStatusEffectsForAllAgents()
        {
            ClearStatusForList(players);
        }
        
        private void ClearStatusForList(List<AgentHealth> list)
        {
            if (list == null) return;

            foreach (var h in list)
            {
                if (h == null || h.Owner == null) continue;

                var controller = h.Owner.GetCompo<StatusEffectController>();
                controller?.ClearAllStatusEffects();
            }
        }
    }
}