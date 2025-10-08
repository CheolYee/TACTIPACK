using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace _00.Work.WorkSpace.Soso7194._04.Scripts.SO
{
    [CreateAssetMenu(fileName = "Enemy", menuName = "SO/Enemy", order = 0)]
    public class EnemySO : ScriptableObject
    {
        public string enemyName;
        public GameObject prefab;
        public int maxHP;
        public int attack;
        
        private void OnValidate()
        {
#if UNITY_EDITOR
            string path = AssetDatabase.GetAssetPath(this);
            AssetDatabase.RenameAsset(path, enemyName);
            AssetDatabase.SaveAssets();
#endif
        }
    }
}