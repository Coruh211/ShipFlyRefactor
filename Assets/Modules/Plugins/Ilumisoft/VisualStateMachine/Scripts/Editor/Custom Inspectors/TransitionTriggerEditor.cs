namespace Ilumisoft.VisualStateMachine
{
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(TransitionTrigger))]
    public class TransitionTriggerEditor : UnityEditor.Editor
    {
        private readonly int LabelWidth = 120;

        SerializedProperty stateMachine;
        SerializedProperty type;
        SerializedProperty key;
        SerializedProperty timeMode;
        SerializedProperty delay;
        SerializedProperty executeOnStart;

        void OnEnable()
        {
            stateMachine = serializedObject.FindProperty("stateMachine");
            type = serializedObject.FindProperty("type");
            key = serializedObject.FindProperty("key");
            timeMode = serializedObject.FindProperty("timeMode");
            delay = serializedObject.FindProperty("delay");
            executeOnStart = serializedObject.FindProperty("executeOnStart");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            //State Machine
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("State Machine", GUILayout.Width(LabelWidth));
            EditorGUILayout.PropertyField(stateMachine, GUIContent.none);
            EditorGUILayout.EndHorizontal();

            //Transition
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Transition", GUILayout.Width(LabelWidth));
            EditorGUILayout.PropertyField(type, GUIContent.none, GUILayout.Width(70));
            EditorGUILayout.PropertyField(key, GUIContent.none);
            EditorGUILayout.EndHorizontal();

            //Time
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Delay", GUILayout.Width(LabelWidth));
            EditorGUILayout.PropertyField(timeMode, GUIContent.none, GUILayout.Width(70));
            EditorGUILayout.PropertyField(delay, GUIContent.none);
            EditorGUILayout.EndHorizontal();

            //Other
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Execute On Start", GUILayout.Width(LabelWidth));
            EditorGUILayout.PropertyField(executeOnStart, GUIContent.none);
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }
    }
}