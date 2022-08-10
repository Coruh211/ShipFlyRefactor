using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FpsController))]
public class FpsControllerInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Apply target FPS"))
            ((FpsController) target).ApplyTargetFps();
    }
}