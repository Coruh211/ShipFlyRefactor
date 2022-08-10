using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace Gameplay.Input
{
    public class InputPrefabSearchHelper : PrefabVisitor
    {
        private SerializedProperty editedPrefabsProperty;
        private List<string> editedPrefabs;
        private List<string> newEditedPrefabs = new List<string>();
        private SerializedObject serializedObject;

        public InputPrefabSearchHelper Init(SerializedObject serializedObject)
        {
            this.serializedObject = serializedObject;

            serializedObject.Update();
            editedPrefabsProperty = serializedObject.FindProperty("editedPrefabs");
            editedPrefabs = editedPrefabsProperty.GetValue<List<string>>();
            types = SearchForTypesOfInteface<IInputHandler>();
            newEditedPrefabs.Clear();

            serializedObject.ApplyModifiedProperties();
            return this;
        }

        public void SearchPrefabs()
        {
            if (editedPrefabsProperty == null)
            {
                Debug.LogError("Helper was not initialized!");
                return;
            }

            if (types == null)
            {
                Debug.LogError("There is no attributes in any script!");
                return;
            }

            string[] folders = serializedObject.FindProperty("searchInFolders").GetValue<string[]>();
            List<string> allPrefabAssetPaths = FindAllPrefabs<IInputHandler>(folders);

            // Validate already subscribed prefabs
            for (int i = editedPrefabs.Count - 1; i >= 0; i--)
            {
                string subscribedPrefabAssetPath = editedPrefabs[i];
                allPrefabAssetPaths.Remove(subscribedPrefabAssetPath);
                SearchPrefab(subscribedPrefabAssetPath);
            }
            // Validate other prefabs except for previous (higher code) prefabs
            for (int i = 0; i < allPrefabAssetPaths.Count; i++)
            {
                string assetPath = allPrefabAssetPaths[i];
                SearchPrefab(assetPath);
            }
        }

        public void SearchPrefab(string assetPath)
        {
            using (var editingScope = new PrefabUtility.EditPrefabContentsScope(assetPath))
            {
                var prefabRoot = editingScope.prefabContentsRoot;
                ApplyInput(prefabRoot, assetPath);
                //Debug.Log("Input in prefab \'" + prefabRoot.name + "\' handled.");
            }
        }

        private void ApplyInput(GameObject prefabRoot, string assetPath)
        {
            List<InputEventData> callbacks = FindInputEventSubscribers(prefabRoot);
            serializedObject.Update();
            EditPrefabs(prefabRoot, callbacks, assetPath);
            editedPrefabsProperty.SetValue(newEditedPrefabs);
            serializedObject.ApplyModifiedProperties();

        }

        private List<InputEventData> FindInputEventSubscribers(GameObject prefabRoot)
        {
            List<IInputHandler> inputHandlers = SearchForHandlers<InputCallbackAttribute, IInputHandler>(prefabRoot);
            InputAttributesResolver[] resolvers = prefabRoot.GetComponentsInChildren<InputAttributesResolver>();
            List<InputEventData> result = attributeSearchHelper.FindAttributes<InputEventData>(inputHandlers);
            RemoveHandledListeners(result);

            return result;
        }

        public static void RemoveHandledListeners(List<InputEventData> callbacks)
        {
            for (int i = 0; i < callbacks.Count; i++)
            {
                GameObject handlerPrefab = PrefabUtility.GetNearestPrefabInstanceRoot(callbacks[i].handlerObject);
                if (handlerPrefab != null && PrefabUtility.HasPrefabInstanceAnyOverrides(handlerPrefab, false))
                {
                    if (handlerPrefab.TryGetComponent(out InputAttributesResolver resolver))
                    {
                        resolver.listeners.Find(listener => listener.methodName == callbacks[i].methodName).removeThisListener = true;
                        string childPrefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(handlerPrefab.transform.GetChild(0));
                        Debug.Log(childPrefabPath);
                    }
                }
            }
        }

        private void EditPrefabs(GameObject prefabRoot, List<InputEventData> callbacks, string assetPath)
        {
            if (callbacks.Count == 0)
            {
                DestroyResolverFromPrefab(prefabRoot);
            }
            else
            {
                if (!prefabRoot.TryGetComponent(out InputAttributesResolver resolver))
                {
                    resolver = prefabRoot.AddComponent<InputAttributesResolver>();
                }
                if (!newEditedPrefabs.Contains(assetPath))
                {
                    newEditedPrefabs.Add(assetPath);
                }

                resolver.listeners.Clear();
                resolver.listeners.AddRange(callbacks);
            }
        }

        private void DestroyResolverFromPrefab(GameObject prefabRoot)
        {
            if (prefabRoot.TryGetComponent(out InputAttributesResolver resolver))
            {
                UnityEngine.Object.DestroyImmediate(resolver);
            }
        }
    }

}
#endif