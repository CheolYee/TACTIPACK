using System.Collections.Generic;
using _00.Work.Scripts.Managers;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Agents;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Events;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.ItemTypes.PassiveItems;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Managers
{
    public class PassiveStatManager : MonoSingleton<PassiveStatManager>
    {
        private readonly HashSet<ItemInstance> _equippedPassives = new();
        
        //SO별 설치 개수 저장
        private readonly Dictionary<PassiveItemSo, int> _counts = new();
        
        private PassiveStatTotals _totals;
        public PassiveStatTotals Totals => _totals;
        
        private void OnEnable()
        {
            Bus<PassiveItemEquippedEvent>.OnEvent += OnEquipped;
            Bus<PassiveItemUnequippedEvent>.OnEvent += OnUnequipped;
        }

        private void OnDisable()
        {
            Bus<PassiveItemEquippedEvent>.OnEvent -= OnEquipped;
            Bus<PassiveItemUnequippedEvent>.OnEvent -= OnUnequipped;
        }
        private void OnEquipped(PassiveItemEquippedEvent evt)
        {
            if (evt.Item == null || evt.Inst == null) return;

            // 이미 카운트 중인 인스턴스(그리드 내 이동 등)이면 무시
            if (!_equippedPassives.Add(evt.Inst))
                return;

            if (!_counts.TryGetValue(evt.Item, out int count))
                count = 0;
            _counts[evt.Item] = count + 1;

            RecalculateTotals();
        }

        private void OnUnequipped(PassiveItemUnequippedEvent evt)
        {
            if (evt.Item == null || evt.Inst == null) return;

            if (!_equippedPassives.Remove(evt.Inst))
                return;

            if (_counts.TryGetValue(evt.Item, out int count))
            {
                count--;
                if (count <= 0)
                    _counts.Remove(evt.Item);
                else
                    _counts[evt.Item] = count;
            }

            RecalculateTotals();
        }
        
        private void RecalculateTotals()
        {
            _totals = new PassiveStatTotals();

            foreach (var kv in _counts)
            {
                PassiveItemSo so = kv.Key;
                int count = kv.Value;

                //0~ 100%로 설정하면 됨
                _totals.hpRate += so.healthMulti  * 0.01f * count;
                _totals.attackRate += so.attackMulti  * 0.01f * count;
                _totals.critAdd += so.critMulti * 0.01f * count;
            }

            ApplyHpToTeam();
        }
        
        private void ApplyHpToTeam()
        {
            var battle = BattleSkillManager.Instance;
            if (battle == null) return;

            //현재 전투에 참가한 플레이어 전원에게 MaxHP 비율 적용
            foreach (AgentHealth hp in battle.GetPlayerTargets())
            {
                if (hp == null) continue;
                hp.SetPassiveHpRate(_totals.hpRate);
            }
        }
        
        public float GetAttackUpFor(Agent agent) => _totals.attackRate;
        public float GetCritAddFor(Agent agent)  => _totals.critAdd;
        public float GetHpRateFor(Agent agent)   => _totals.hpRate;
    }
}