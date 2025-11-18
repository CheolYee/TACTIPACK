using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace _00.Work.WorkSpace.Soso7194._04.Scripts.SO
{
    [CreateAssetMenu(fileName = "Enemy", menuName = "SO/Enemy", order = 0)]
    public class EnemySO : ScriptableObject
    {
        [Header("Enemy Setting")]
        public string enemyName; // 이름
        public GameObject prefab; // 에너미 프리팹
        public int maxHP; // 체력
        public int attack; // 공격력

        [Header("Boss Setting")] 
        public bool isBoss;
        public int skillTurn;
        public int skillDamage;
        
        private void OnValidate()
        {
            if(enemyName == null) return;
            //인스펙터에서 enemyName 바꾸면 인스팩터 이름 변경
#if UNITY_EDITOR
            string path = AssetDatabase.GetAssetPath(this);
            AssetDatabase.RenameAsset(path, enemyName);
            AssetDatabase.SaveAssets();
#endif
        }
    }
}