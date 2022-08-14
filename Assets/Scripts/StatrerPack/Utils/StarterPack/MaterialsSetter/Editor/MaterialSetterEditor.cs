using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MatEditor.Editors
{
    [CustomEditor(typeof(MaterialSetter))]
    public class MaterialSetterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("To Cash"))
            {
                foreach (var tgObj in serializedObject.targetObjects)
                {
                    if (tgObj is MaterialSetter setter)
                    {
                        setter.ToHash();
                    }
                }

                serializedObject.Update();
            }
            if (GUILayout.Button("Load Materials"))
            {
                foreach (var tgObj in serializedObject.targetObjects)
                {
                    if (tgObj is MaterialSetter setter)
                    {
                        setter.ApplyAllMaterials();
                    }
                }

                serializedObject.Update();
            }
            GUILayout.EndHorizontal();
        }
    }
}
