using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.FSMSystem.Editor
{
    [CustomEditor(typeof(StateSo))] //어떤거에대한 에디터를 만들래
    public class StateSoEditor : UnityEditor.Editor
    {
        [SerializeField] private VisualTreeAsset stateView = default;
        
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            stateView.CloneTree(root);
            

            DropdownField dropdownField = root.Q<DropdownField>("ClassDropDownField");
            
            CreateDropDownList(dropdownField);

            dropdownField.RegisterValueChangedCallback(HandleDropDownFieldChange);
            return root;
        }

        private void HandleDropDownFieldChange(ChangeEvent<string> evt)
        {
            StateSo targetData = target as StateSo;
            targetData.className = evt.newValue;
            
            EditorUtility.SetDirty(targetData); //수정된 놈들 플래그 박아주기
            AssetDatabase.SaveAssets(); //플래그 박아진것들만 저장하기
        }

        private void CreateDropDownList(DropdownField targetField)
        {
            targetField.choices.Clear();
            
            Assembly stateAssembly = Assembly.GetAssembly(typeof(AgentState));
            
            List<Type> derivedTypes = stateAssembly.GetTypes()
                .Where(type => type.IsClass
                               && type.IsAbstract == false
                               && type.IsSubclassOf(typeof(AgentState)))
                .ToList();
            
            derivedTypes.ForEach(type => targetField.choices.Add(type.FullName));
            
            if (targetField.choices.Count > 0)
                targetField.SetValueWithoutNotify(derivedTypes[0].FullName);
        }
    }
}