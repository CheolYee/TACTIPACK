using System.Collections;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Agents;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.ItemTypes.ActiveItems;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Attacks
{
    //공격을 담당하는 컴포넌트
    public class AttackExecutor : MonoBehaviour, IAgentComponent
    {
        private Agent _agent;
        public void Initialize(Agent agent) //Agent에서 초기화될 메서드
        {
            _agent = agent;
        }
        
        
        /// <summary>
        /// 아이템을 실행하고 스킬 완료까지 대기한다.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public IEnumerator Perform(AttackItemSo item, GameObject target = null)
        {
            if (item == null || target == null) yield break;
            
            Debug.Log("스킬은 실행");
            
            ISkillHandler skillHandler = item.ActiveAndGetSkillHandler(_agent.gameObject, target);
            if (skillHandler != null)
            {
                bool done = false;
                void OnDone() => done = true;
                skillHandler.OnComplete += OnDone;
                //완료까지 대기
                while (!done) yield return null;
                skillHandler.OnComplete -= OnDone;
            }
            else
            {
                //핸들이 없으면 SkillDelay로 폴백
                float t = 0f;
                while (t < item.SkillDelay)
                {
                    t += Time.deltaTime;
                    yield return null;
                }
            }
        }
    }
}