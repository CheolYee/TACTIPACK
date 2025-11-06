using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Attacks
{
    [CreateAssetMenu(fileName = "AttackData", menuName = "SO/Attack/AttackData", order = 0)]
    public class AttackDataSo : ScriptableObject
    {
        [field: SerializeField] public float Damage { get; private set; }
    }
}