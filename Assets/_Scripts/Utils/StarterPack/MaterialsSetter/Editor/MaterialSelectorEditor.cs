using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MatEditor.Editors
{
    [CustomEditor(typeof(MaterialSelector))]
    public class MaterialSelectorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            string selPattern = MaterialSelector.SelectedPattern;
            GUILayout.Label($"Pattern: {selPattern}");

            base.OnInspectorGUI();

            if (GUILayout.Button("Rehash"))
            {
                (serializedObject.targetObject as MaterialSelector).Rehash();
            }
            if (GUILayout.Button($"Load [{selPattern}] in Scene"))
            {
                (serializedObject.targetObject as MaterialSelector).LoadMaterialsPattern();
            }
        }
    }
}
