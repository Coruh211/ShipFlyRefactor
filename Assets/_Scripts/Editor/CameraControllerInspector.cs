using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CameraController))]
public class CameraControllerInspector : Editor
{
    private SerializedProperty targetObject;
    private SerializedProperty offset;
    
    private void OnEnable()
    {
        targetObject = serializedObject.FindProperty("target");
        offset = serializedObject.FindProperty("offset");
    }
    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Set current offset"))
        {
            if (!targetObject.objectReferenceValue)
                return;
            
            serializedObject.Update();
            offset.vector3Value = -((Transform) targetObject.objectReferenceValue).position
                                  + ((CameraController) target).transform.position;
            serializedObject.ApplyModifiedProperties();
        }
        
        if (GUILayout.Button("Set offset to position"))
        {
            if (!targetObject.objectReferenceValue)
                return;

            ((CameraController) target).gameObject.transform.position = ((Transform) targetObject.objectReferenceValue).position + offset.vector3Value;
        }
    }
}