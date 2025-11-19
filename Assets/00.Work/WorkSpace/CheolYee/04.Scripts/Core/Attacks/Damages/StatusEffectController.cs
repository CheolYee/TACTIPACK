using System.Collections.Generic;
using System.Linq;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Agents;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.ItemTypes.ActiveItems;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Attacks.Damages
{
    public class StatusEffectController : MonoBehaviour, IAgentComponent
    {
        private Agent _owner;
        private AgentHealth _health;
        
        private readonly List<StatusEffectInstance> _effects = new();
        
        public bool IsStunned =>
            _effects.Any(e => e.Type == StatusEffectType.Stun && e.RemainingTurns > 0);
        public void Initialize(Agent agent)
        {
            _owner = agent;
            _health = agent.Health;
        }
        
        /// <summary>
        /// 스킬에서 상태이상 걸 때 사용함
        /// </summary>
        public void AddStatus(StatusEffectInfo info)
        {
            //만약 없거나 지속시간이 0보다 작거나 같으면 리턴
            if (info.type == StatusEffectType.None || info.durationTurns <= 0) return;
            
            //만약 현재 있는 상태이상중 같은게 있는지 확인 후 저장
            StatusEffectInstance existing = _effects.FirstOrDefault(e => e.Type == info.type);

            if (existing != null)
            {
                //만약 중첩 가능이라면
                if (info.stackable)
                {
                    //중첩 개수 올려주고, 지속 턴을 합쳐요
                    existing.StackCount++;
                }
                else //중첩 불가라면 남은 턴과 파워 갱신만함
                {
                    existing.RemainingTurns = Mathf.Max(existing.RemainingTurns, info.durationTurns);
                    existing.Power = Mathf.Max(existing.Power, info.power);
                }
            }
            else
            {
                _effects.Add(new StatusEffectInstance(info.type, info.durationTurns, info.power));
            }
        }

        /// <summary>
        /// 자신 턴 시작 시 호출
        /// </summary>
        public bool OnTurnStart()
        {
            var stun = _effects.FirstOrDefault(e => 
                e.Type == StatusEffectType.Stun 
                && e.RemainingTurns > 0);

            if (stun != null)
            {
                //만약 상태 이상 중 기절 상태면 여기서 True반환 후 턴 소모
                stun.RemainingTurns--;
                Cleanup();
                return true;
            }
            
            return false;
        }

        public void OnTurnEnd()
        {
            foreach (var effect in _effects)
            {
                switch (effect.Type)
                {
                    case StatusEffectType.Bleed:
                        if (effect.RemainingTurns > 0 && _health != null)
                        {
                            //턴이 존재하고 체력이 있으면
                            //파워와 스택 쌓인 개수만큼 고정 피해
                            float damage = effect.Power * effect.StackCount;
                            _health.ApplyDamage(new DamageContainer(damage));
                        }
                        break;
                }
                
                //턴당 감소가 되는 애들 감소시키기
                if (effect.Type == StatusEffectType.Bleed ||
                    effect.Type == StatusEffectType.Burn)
                {
                    effect.RemainingTurns--;
                }
            }
            
            Cleanup();
        }
        
        //화상 추가피해
        public void OnDamaged(DamageContainer attackData)
        {
            if (_health == null) return;

            StatusEffectInstance burn = 
                _effects.FirstOrDefault(e 
                    => e.Type == StatusEffectType.Burn && e.RemainingTurns > 0);
            
            if (burn != null)
            {
                // 예시: 공격 데미지의 (Power%) * 스택 만큼 추가 피해
                float extra = attackData.Damage * burn.Power * burn.StackCount;
                _health.ApplyDamage(new DamageContainer(extra));
            }
        }
        

        private void Cleanup()
        {
            _effects.RemoveAll(e => e.RemainingTurns <= 0);
        }
    }
}