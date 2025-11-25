using System.Collections;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Agents;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Attacks.Skills;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.ItemTypes.ActiveItems;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Managers;
using _00.Work.WorkSpace.CheolYee._04.Scripts.UI;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Attacks
{
    //공격을 담당하는 컴포넌트
    public class AttackExecutor : MonoBehaviour, IAgentComponent
    {
        private Agent _agent; //부모 agent\
        private BattleSkillManager _skillManager;
        public void Initialize(Agent agent) //Agent에서 초기화될 메서드
        {
            _agent = agent; //주인
            _skillManager = BattleSkillManager.Instance; //씬에 존재하는 매니저 가져오기
        }


        /// <summary>
        /// 아이템을 실행하고 스킬 완료까지 대기한다.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="overrideStance"></param>
        /// <returns></returns>
        public IEnumerator Perform(AttackItemSo item, AttackStance? overrideStance = null)
        {
            if (item == null) yield break; //아이템이 없다면 탈출
            
            SkillContent ctx = _skillManager.BuildContext(_agent, item, overrideStance); //싱글톤으로 스킬 컨텐츠 가져오기
            yield return PerformInternal(item, ctx);
        }
        
        public IEnumerator Perform(AttackItemSo item, SkillContent ctx)
        {
            if (item == null || ctx == null) yield break;
            yield return PerformInternal(item, ctx);
        }

        private IEnumerator PerformInternal(AttackItemSo item, SkillContent ctx)
        {
            ISkillHandler handle = item.ActiveWithSkillContent(ctx);

            if (handle != null)
            {
                bool done = false;
                void OnDone() => done = true;

                handle.OnComplete += OnDone;

                while (!done)
                    yield return null;

                handle.OnComplete -= OnDone;
            }

            float t = 0f;
            while (t < item.SkillDelay)
            {
                t += Time.deltaTime;
                yield return null;
            }
        }
    }
}