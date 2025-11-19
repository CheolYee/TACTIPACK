using System.Collections.Generic;
using _00.Work.Scripts.Managers;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Events;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Managers
{
    public class ItemCooldownManager : MonoSingleton<ItemCooldownManager>
    {
        private readonly HashSet<ItemInstance> _trackedInstances = new();
        private readonly HashSet<ItemInstance> _justStartedThisRound = new();
        

        private void OnEnable()
        {
            Bus<BattleRoundAdvancedEvent>.OnEvent += OnBattleRoundAdvanced;
            Bus<OnItemReturnedToSideInventory>.OnEvent += Unregister;
        }

        private void OnDisable()
        {
            Bus<BattleRoundAdvancedEvent>.OnEvent -= OnBattleRoundAdvanced;
            Bus<OnItemReturnedToSideInventory>.OnEvent -= Unregister;
        }

        public void Register(ItemInstance item)
        {
            if (item == null) return;
            _trackedInstances.Add(item);
            _justStartedThisRound.Add(item);
        }

        public void Unregister(OnItemReturnedToSideInventory item)
        {
            if (item.Inst == null) return;
            _trackedInstances.Remove(item.Inst);
            _justStartedThisRound.Remove(item.Inst);
        }

        //그리드에 있는 모든 아이템의 쿨타임 설정
        private void OnBattleRoundAdvanced(BattleRoundAdvancedEvent evt)
        {
            foreach (ItemInstance inst in _trackedInstances)
            {
                //이번 라운드에 쿨타임이 막 시작된 애면, 이번에는 깎지 않는다
                if (_justStartedThisRound.Contains(inst))
                    continue;

                inst.TickCooldownOneTurn();
            }

            //한 라운드가 끝났으니, "이번 라운드에 시작된 애" 마크는 초기화
            _justStartedThisRound.Clear();
        }
    }
}