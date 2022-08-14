using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LauncherManager))]
public class LauncherManagerInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Find All Components"))
            ((LauncherManager) target).FindAllComponents();
    }
}