using _00.Work.Resource.Scripts.Managers;
using _00.Work.Scripts.Managers;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Attacks.Damages
{
    public enum DamageTextKind
    {
        Normal, //일반 공격
        Bleed, //출혈
        Burn, //화상
        Heal, //힐
        Barrier //베리어
    }
    public class DamageTextSpawner : MonoSingleton<DamageTextSpawner>
    {
        [SerializeField] private string poolKey = "DamageText";

        public void Spawn(float damage, bool isCritical, Vector3 worldPos,
            DamageTextKind kind = DamageTextKind.Normal)
        {
            if (PoolManager.Instance == null)
                return;

            var pooled = PoolManager.Instance.Pop(poolKey) as DamageTextUi;
            if (pooled == null)
                return;

            pooled.Play(damage, isCritical, worldPos, kind);
        }
    }
}