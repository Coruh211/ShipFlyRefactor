using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR


namespace Gameplay.Input
{
    [ExecuteInEditMode]
    [InitializeOnLoad]
    public class InputSceneManager : MonoBehaviour
    {        
        protected static InputSceneManager Instance
        {
            get; private set;
        }

        private SerializedProperty settingsProperty;
        private SerializedObject serializedObject;

        private InputListenersManager Settings {
            get { 
                if (serializedObject == null)
                    serializedObject = new SerializedObject(GetComponent<InputEventManager>());
                if (settingsProperty == null)
                    settingsProperty = serializedObject.FindProperty(nameof(InputEventManager.Settings));

                return Instance.settingsProperty.GetValue<InputListenersManager>();
            }
        }

        private void Update()
        {
            if (!EditorApplication.isPlaying && UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() != null && UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage().scene.isDirty)
            {
                UnityEditor.SceneManagement.PrefabStage prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
                PrefabUtility.SaveAsPrefabAsset(prefabStage.prefabContentsRoot, prefabStage.assetPath);
                prefabStage.ClearDirtiness();
                Settings?.FindInPrefab(prefabStage.assetPath);
            }
        }

        [InitializeOnLoadMethod]
        static void OnReload()
        {
            if (!Instance)
            {
                Instance = FindObjectOfType<InputSceneManager>();
                if (Instance == null)
                {
                    return;
                }
            }

            if (Instance.Settings == null)
            {
                Instance.FindSettingsAsset();
            }
            UpdateOnAnyDirtiness();
        }
        private static void UpdateOnAnyDirtiness()
        {
            if (EditorApplication.isPlaying)
            {
                return;
            }

            Instance.Settings.FindInScene();
            Instance.Settings.FindInPrefabs();

            PrefabUtility.prefabInstanceUpdated = Instance.UpdateInputPrefabs;
            EditorApplication.playModeStateChanged += OnPlayMode;
            Debug.Log("Input handled.");
        }

        public static void OnPlayMode(PlayModeStateChange state)
        {            
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                InputAttributesResolver[] resolversInScene = FindObjectsOfType<InputAttributesResolver>();
                for (int i = 0; i < resolversInScene.Length; i++)
                {
                    InputAttributesResolver resolver = resolversInScene[i];
                    if (PrefabUtility.GetNearestPrefabInstanceRoot(resolver) == null)
                    {
                        DestroyImmediate(resolver);
                    }
                }
                Instance.Settings.FindInScene();
                Instance.Settings.FindInPrefabs();
            }            
        }

        private void UpdateInputPrefabs(GameObject instance)
        {
            Settings.FindInPrefab(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(instance));
            Settings.FindInScene();
        }
        private void FindSettingsAsset()
        {
            string settingsPath = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets($"t:{nameof(InputListenersManager)}", null)[0]);
            SerializedPropertyExtensions.SetFieldValue(nameof(InputEventManager.Settings), serializedObject.targetObject, AssetDatabase.LoadAssetAtPath(settingsPath, typeof(InputListenersManager)));
        }
    }
}
#endif