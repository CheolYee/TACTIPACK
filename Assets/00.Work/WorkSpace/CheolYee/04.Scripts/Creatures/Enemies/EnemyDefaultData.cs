using System.Collections.Generic;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.ItemTypes.ActiveItems;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Creatures.Enemies
{
    [CreateAssetMenu(fileName = "EnemyDefaultData", menuName = "SO/Character/EnemyDefaultData", order = 0)]
    public class EnemyDefaultData : ScriptableObject
    {
        [Header("Info")]
        [field: SerializeField] public string EnemyName {get; private set;} //에너미의 이름
        [field: SerializeField] public int EnemyId {get; private set;} //에너미의 아이디
        [field: SerializeField] public RuntimeAnimatorController AnimatorController {get; private set;}
        [field: SerializeField] public Vector2 CharacterOffset {get; private set;}
        
        [Header("Settings")]
        [field: SerializeField] public float MaxHp {get; private set;} //HP
        [field: SerializeField] public float DefaultDamage {get; private set;} //기본 데미지
        [field: SerializeField] public float DefaultCritChance {get; private set;} //크리티컬 확률
        [field: SerializeField] public List<AttackItemSo> Attacks {get; private set;}
    }
}