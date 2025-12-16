using System;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Agents
{
    public class AgentTooltipStats : MonoBehaviour, IAgentComponent, IAfterInitialize
    {
        private Agent _owner;
        private AgentHealth _health;

        private float _currentHp;
        private float _maxHp;
        
        public float CurrentHp => _currentHp;
        public float MaxHp => _maxHp;
        public void Initialize(Agent agent)
        {
            _owner = agent;   
        }

        public void AfterInitialize()
        {
            if (_owner == null) return;
            
            _health = _owner.Health;
            if (_health != null)
            {
                _health.OnInitHealth += OnHealthChanged;
                _health.OnHealthChange += OnHealthChanged;
                
                _maxHp = _health.MaxHealth;
                _currentHp = _health.CurrentHealth;
            }
        }

        private void OnDestroy()
        {
            if (_health != null)
            {
                _health.OnInitHealth -= OnHealthChanged;
                _health.OnHealthChange -= OnHealthChanged;
            }
        }

        private void OnHealthChanged(float prevhealth, float currenthealth, float maxhealth)
        {
            _currentHp = currenthealth;
            _maxHp = maxhealth;
        }

        public float GetFinalCritChance01()
        {
            if (_owner == null) return 0;
            
            float baseCrit = _owner.GetBaseCritChance();

            float extraCrit = 0f;
            if (_owner.StatusEffectController != null)
            {
                extraCrit = _owner.StatusEffectController.GetCritChanceModifier();
            }
            
            return Mathf.Clamp01(baseCrit + extraCrit);
        }
    }
}