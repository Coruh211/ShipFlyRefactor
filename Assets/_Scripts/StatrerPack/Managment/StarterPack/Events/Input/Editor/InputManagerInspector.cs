using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gameplay.Input
{
    [CustomEditor(typeof(InputListenersManager))]
    [ExecuteInEditMode]
    public class InputManagerInspector : Editor
    {
        private InputPrefabSearchHelper prefabSearchHelper = new InputPrefabSearchHelper();

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Search"))
            {
                prefabSearchHelper.Init(serializedObject).SearchPrefabs();
                (target as InputListenersManager).FindInScene();
            }
        }
    }
}