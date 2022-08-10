using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Gameplay.Input
{
    [CreateAssetMenu(fileName = "InputListenersManager", menuName = "Input/Listeners Manager")]
    [ExecuteInEditMode]
    public class InputListenersManager : ScriptableObject
    {
        private InputAttributesResolver resolver;
        [field: SerializeField]
        public bool MultitouchEnabled
        {
            get; private set;
        }

# if UNITY_EDITOR
        private AttributeSearchHelper<IInputHandler, InputCallbackAttribute> attributeSearchHelper = new AttributeSearchHelper<IInputHandler, InputCallbackAttribute>();
        private InputPrefabSearchHelper prefabSearchHelper = new InputPrefabSearchHelper();
        private SerializedObject resolverInScene;

        public List<string> editedPrefabs = new List<string>();
        public string[] searchInFolders = { "Assets/Prefabs" };


        public void FindInScene()
        {
            if (!resolver)
            { 
                resolver = FindResolverInScene();
            }
            resolverInScene = new SerializedObject(resolver);
            resolverInScene.Update();
            SerializedPropertyExtensions.SetFieldValue(nameof(InputAttributesResolver.listeners), resolver, new List<InputEventData>());
            resolverInScene.ApplyModifiedProperties();
            List<InputEventData> listeners = attributeSearchHelper.FindAttributes<InputEventData>(FindAllSubscribers());

            SerializedPropertyExtensions.SetFieldValue(nameof(InputAttributesResolver.listeners), resolver, listeners);
            resolverInScene.ApplyModifiedProperties();
        }

        public void FindInPrefab(string assetPath)
        {
            prefabSearchHelper.Init(new SerializedObject(this)).SearchPrefab(assetPath);
        }

        public void FindInPrefabs()
        {
            prefabSearchHelper.Init(new SerializedObject(this)).SearchPrefabs();
        }

        private InputAttributesResolver FindResolverInScene()
        {
            InputAttributesResolver resolver = SceneManager.GetActiveScene().GetRootGameObjects()[0].GetComponent<InputAttributesResolver>();
            if (resolver == null)
            {
                resolver = SceneManager.GetActiveScene().GetRootGameObjects()[0].AddComponent<InputAttributesResolver>();
            }
            return resolver;
        }

        private List<IInputHandler> FindAllSubscribers()
        {
            List<IInputHandler> inputHandlers = new List<IInputHandler>();
            foreach (GameObject rootGameObject in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                List<MonoBehaviour> monos = new List<MonoBehaviour>(rootGameObject.GetComponentsInChildren<MonoBehaviour>());
                monos.FindAll(x => x.GetType().GetInterfaces().Contains(typeof(IInputHandler))).ForEach(x => inputHandlers.Add(x as IInputHandler));
            }
            return inputHandlers;
        }
#endif
    }
}