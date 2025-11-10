// Editor/ItemDataSOEditor.cs
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.Editor
{
    [CustomEditor(typeof(ItemDataSo))]
    public class ItemDataSoEditor : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            ItemDataSo so = (ItemDataSo)target;
            if(string.IsNullOrEmpty(so.itemId)) {
                if(GUILayout.Button("아이디 생성")) {
                    string path = AssetDatabase.GetAssetPath(so);
                    string guid = AssetDatabase.AssetPathToGUID(path);
                    so.itemId = guid;
                    EditorUtility.SetDirty(so);
                }
            } else {
                if(GUILayout.Button("아이디 클립보드 카피")) {
                    EditorGUIUtility.systemCopyBuffer = so.itemId;
                }
            }
        }
    }
}
#endif