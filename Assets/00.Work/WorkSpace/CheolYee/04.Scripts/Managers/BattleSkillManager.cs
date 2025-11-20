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
            list = list.Where(h => h != null).ToList();
            if (list.Count == 0) return null;

            int idx = Random.Range(0, list.Count);
            return list[idx];
        }

        public SkillContent BuildContext(Agent user, AttackItemSo item, AttackStance? overrideStance = null)
        {
            List<AgentHealth> allTargets;
            
            if (item != null && item.IsHealSkill)
                allTargets = GetAlliesOf(user); //힐 스킬 
            else
                allTargets = GetOpponentsOf(user); //공격/디버프
            
            var ctx = new SkillContent()
            {
                User = user,
                TargetingMode = item.TargetingMode,
                Stance = overrideStance ?? AttackStance.Stationary,
                ApproachOffset = item.ApproachOffset,
                Targets = new List<AgentHealth>()
            };
            
            if (allTargets.Count == 0)
            {
                ctx.CastPoint = user.transform.position;
                return ctx;
            }
            
            //여기서 타겟팅 모드에 따라서 타겟 설정도 같이함
            switch (item.TargetingMode)
            {
                //단일 타겟 (원하는 인덱스)
                case TargetingMode.Single:
                {
                    //최소 0 최대는 모든 타겟 -1(인덱스 변환)을 넘지 않도록 해준다. (실수방지)
                    int idx = Mathf.Clamp(item.TargetIndex, 0, allTargets.Count - 1);
                    //타겟 설정한다
                    AgentHealth target = allTargets[idx];

                    //원하는 한 타겟만 컨테이너에 담는다
                    ctx.Targets.Add(target);
                    //그 놈 위치도 담는다
                    ctx.CastPoint = target.transform.position;

                    //타겟 된 놈만 체력바 보여준다
                    HudManager.Instance.ShowOnly(target.HealthBarInstance);
                    break;
                }

                //범위 공격 (전체임)
                case TargetingMode.Area:
                {
                    //모든 타겟을 추가한다
                    ctx.Targets.AddRange(allTargets);
                    ctx.CastPoint = GetCenterPoint(allTargets);

                    //모든 타겟의 전체 체력을 다 가져오기
                    List<HealthBarUi> bars = allTargets.Select(h => h.HealthBarInstance).ToList();
                    //모든 타겟을 때려박아 HP 보여주기
                    HudManager.Instance.ShowOnly(bars);
                    break;
                }

                //랜덤 한놈 선택
                case TargetingMode.Random:
                {
                    //모든 타겟의 개수만큼 인덱스를 랜덤으로 빼온다
                    int idx = Random.Range(0, allTargets.Count);
                    //타겟을 설정한다
                    AgentHealth target = allTargets[idx];
                    
                    //설정한 타겟을 컨테이너에 담는다.
                    ctx.Targets.Add(target);
                    //타겟 위치도 담는다.
                    ctx.CastPoint = target.transform.position;

                    //타겟된 놈만 보여준다
                    HudManager.Instance.ShowOnly(target.HealthBarInstance);
                    break;
                }

                //자기 자신 타겟
                case TargetingMode.Self:
                default:
                {
                    //자기 위치를 스킬 캐스팅 위치로 한다
                    ctx.CastPoint = user.transform.position;

                    if (user.Health != null)
                    {
                        //타겟 컨테이너에 자신을 추가한다 (혹시 모를 자가치유 스킬 위해)
                        ctx.Targets.Add(user.Health);
                        //자기 자신을 보여준다.
                        HudManager.Instance.ShowOnly(user.Health.HealthBarInstance);
                    }
                    break;
                }
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
    }
}