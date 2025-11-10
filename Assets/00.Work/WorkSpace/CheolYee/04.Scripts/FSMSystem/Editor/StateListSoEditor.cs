using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.FSMSystem.Editor
{
    public class CodeFormat
    {
        public static string EnumFormat =
            @"
namespace _00.Work.WorkSpace.CheolYee._04.Scripts.FSMSystem
{{
    public enum {0}
    {{
        {1}
    }}
}}
";
    }
    
    [CustomEditor(typeof(StateListSo))]
    public class StateListSoEditor : UnityEditor.Editor
    {
        [SerializeField] private VisualTreeAsset stateListView = default;
        
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            InspectorElement.FillDefaultInspector(root, serializedObject, this);
            stateListView.CloneTree(root);

            root.Q<Button>("GenerateButton").clicked += HandleGenerateButtonClick;
            
            return root;
        }

        private void HandleGenerateButtonClick() //버튼 클릭하면 자동 이넘생성
        {
            StateListSo list = target as StateListSo;

            int index = 0;
            string enumString = string.Join(", ", list.states.Select(so =>
            {
                so.stateIndex = index;
                EditorUtility.SetDirty(so);
                return $"{so.stateName} = {index++}";
            }));

            string scriptPath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
            string dirName= Path.GetDirectoryName(scriptPath);
            DirectoryInfo parentDirectory = Directory.GetParent(dirName);
            string path = parentDirectory.FullName;
            string code = string.Format(CodeFormat.EnumFormat, list.enumName, enumString);
            
            File.WriteAllText($"{path}/{list.enumName}.cs", code);

            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}