using System;
using System.Collections.Generic;
using System.Linq;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Agents;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.ItemTypes.ActiveItems;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Attacks.Damages
{
    public class StatusEffectController : MonoBehaviour, IAgentComponent, IAfterInitialize
    {
        private Agent _owner;
        private AgentHealth _health;
        
        private readonly List<StatusEffectInstance> _effects = new();
        
        public IReadOnlyList<StatusEffectInstance> ActiveEffects => _effects;
        
        public event Action<StatusEffectController> OnStatusChanged;
        public bool IsStunned =>
            _effects.Any(e => e.Type == StatusEffectType.Stun && e.RemainingTurns > 0);
        
        private bool _blockedLastDamage; //보호막이 공격을 방어했는가?
        
        public void Initialize(Agent agent)
        {
            _owner = agent;
        }
        
        public void AfterInitialize()
        {
            _health = _owner.Health;
        }
        
        /// <summary>
        /// 데미지 계산기
        /// </summary>
        public float GetAttackRate()
        {
            float up = 0f;
            float down = 0f;

            foreach (StatusEffectInstance e in _effects)
            {
                if (e.RemainingTurns <= 0) continue;

                switch (e.Type)
                {
                    case StatusEffectType.AttackUp:
                        up += e.Power * e.StackCount;
                        break;
                    case StatusEffectType.AttackDown:
                        down += e.Power * e.StackCount;
                        break;
                }
            }

            float result = 1f + up - down;
            //공격력 0 밑으로는 내려가면 안돼요!!!!!!!!!!!!!!!!!!!!!!!
            return Mathf.Max(0f, result);
        }
        
        /// <summary>
        /// 치명타 확률 보정치 (0.15f = +15%)
        /// 합연산
        /// </summary>
        public float GetCritChanceModifier()
        {
            float up = 0f;
            float down = 0f;

            foreach (var e in _effects)
            {
                if (e.RemainingTurns <= 0) continue;

                switch (e.Type)
                {
                    case StatusEffectType.CritUp:
                        up += e.Power * e.StackCount;
                        break;
                    case StatusEffectType.CritDown:
                        down += e.Power * e.StackCount;
                        break;
                }
            }

            return up - down;
        }
        
        
        /// <summary>
        /// 스킬에서 상태이상 걸 때 사용함
        /// </summary>
        public void AddStatus(StatusEffectInfo info)
        {
            //만약 없거나 지속시간이 0보다 작거나 같으면 리턴
            if (info.type == StatusEffectType.None) return;
            
            //남은 턴이 0보다 작다면
            if (info.durationTurns <= 0)
                return;
            
            //보호막이 막았으면
            if (_blockedLastDamage && IsDebuff(info.type)) return;
            
            //만약 현재 있는 상태이상중 같은게 있는지 확인 후 저장
            StatusEffectInstance existing = _effects.FirstOrDefault(e => e.Type == info.type);

            if (existing != null)
            {
                if (info.type == StatusEffectType.Barrier)
                {
                    //리메이닝 턴을 남은 턴수로
                    existing.RemainingTurns += info.durationTurns;
                    existing.Power = info.power; //필요하면 파워도 갱신
                }
                //만약 중첩 가능이라면
                if (info.stackable)
                {
                    //중첩 개수 올려주고, 지속 턴을 합쳐요
                    existing.StackCount++;
                }
                else //중첩 불가라면 남은 턴과 파워 갱신만
                {
                    existing.RemainingTurns = Mathf.Max(existing.RemainingTurns, info.durationTurns);
                    existing.Power = Mathf.Max(existing.Power, info.power);
                }
            }
            else
            {
                _effects.Add(new StatusEffectInstance(info.type, info.durationTurns, info.power));
            }
            RaiseStatusChanged();
            
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
                RaiseStatusChanged();
                return true;
            }
            
            return false;
        }

        public void OnTurnEnd()
        {
            _blockedLastDamage = false;
            
            foreach (var effect in _effects)
            {
                switch (effect.Type)
                {
                    case StatusEffectType.Bleed:
                        if (effect.RemainingTurns > 0 && _health != null)
                        {
                            //턴이 존재하고 체력이 있으면
                            //파워와 스택 쌓인 개수만큼 최대체력 비례 데미지
                            float ratio = effect.Power; 
                            float damage = _health.MaxHealth * ratio * effect.StackCount;
                            _health.ApplyDirectDamage(damage);
                        }
                        break;
                }
                
                //턴당 감소가 되는 애들 감소시키기
                if (effect.Type == StatusEffectType.Bleed ||
                    effect.Type == StatusEffectType.Burn  ||
                    effect.Type == StatusEffectType.AttackUp  ||
                    effect.Type == StatusEffectType.AttackDown||
                    effect.Type == StatusEffectType.CritUp    ||
                    effect.Type == StatusEffectType.CritDown)
                {
                    effect.RemainingTurns--;
                }
            }
            
            Cleanup();
            RaiseStatusChanged();
        }
        
        //화상 추가피해
        public void OnDamaged(DamageContainer attackData)
        {
            if (_health == null) return;
            if (attackData.Damage <= 0) return;

            StatusEffectInstance burn = 
                _effects.FirstOrDefault(e 
                    => e.Type == StatusEffectType.Burn && e.RemainingTurns > 0);
            
            if (burn != null)
            {
                // 예시: 공격 데미지의 (Power%) * 스택 만큼 추가 피해
                float extra = attackData.Damage * burn.Power * burn.StackCount;
                _health.ApplyDirectDamage(extra);
            }
        }

        private void RaiseStatusChanged()
        {
            OnStatusChanged?.Invoke(this);
        }

        private void Cleanup()
        {
            _effects.RemoveAll(e => e.RemainingTurns <= 0);
        }

        //베리어 처리
        public bool TryBlockDamage(ref DamageContainer attackData)
        {
            _blockedLastDamage = false;
            
            //살아있는 베리어 찾기
            var barrier = _effects.FirstOrDefault(e =>
                e.Type == StatusEffectType.Barrier && e.RemainingTurns > 0);

            if (barrier == null)
                return false;

            //히트 하나 소모
            barrier.RemainingTurns--;

            //아이콘과 리스트 정리
            if (barrier.RemainingTurns <= 0)
            {
                Cleanup();
            }
            RaiseStatusChanged(); //UI 아이콘 갱신

            //실제 데미지는 0으로
            attackData.Damage = 0f;
            attackData.IsCritical = false; // 크리라 해도 막혔으니 의미 없음

            _blockedLastDamage = true;
            
            return true;
        }
        
        //받은 타입이 디버프인지 판별하기
        private bool IsDebuff(StatusEffectType type)
        {
            switch (type)
            {
                case StatusEffectType.Bleed:
                case StatusEffectType.Burn:
                case StatusEffectType.Stun:
                case StatusEffectType.AttackDown:
                case StatusEffectType.CritDown:
                    return true;
                default:
                    return false;
            }
        }
    }
}