using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Menus.Editors
{
    [CustomEditor(typeof(GUIManager))]
    public class GUIManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if(GUILayout.Button("Find all menus in scene"))
            {
                GUIManager.Instance.FindAllMenusOnSceneAndCash();
            }
        }
    }
}
