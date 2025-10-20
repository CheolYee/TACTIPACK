
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.ItemTypes.ActiveItems
{
    [CreateAssetMenu(fileName = "newAttackItem", menuName = "SO/Item/ActiveItem/AttackItem", order = 0)]
    public class AttackItemSo : ActiveItemDataSo
    {
        [Header("Attack Type")]
        public bool isAreaAttack;
        public float areaRadius;

        public override void Activate(GameObject user, GameObject target = null)
        {
            if (skillPrefab == null) return;

            if (isAreaAttack)
            {
                //범위 공격
                GameObject aoe = Instantiate(skillPrefab, target.transform.position, Quaternion.identity);
            }
            else
            {
                //단일 타겟 공격
                Vector3 pos = user.transform.position;
                GameObject proj = Instantiate(skillPrefab, pos, Quaternion.identity);
            }
        }
    }
}