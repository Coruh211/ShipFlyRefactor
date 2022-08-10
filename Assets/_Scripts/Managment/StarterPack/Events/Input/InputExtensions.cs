using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Gameplay.Input;
using System;
using UnityEngine.SceneManagement;

public static class InputExtensions
{
    public static void UnsubscribeInput(this MonoBehaviour script, string methodName, Transform t = null)
    {
        if (TrySearchForScript(t != null ? t : script.transform, out InputAttributesResolver resolver))
        {
            if (!resolver.RemoveSubscription(script, methodName) && resolver.transform)
            {
                script.UnsubscribeInput(methodName, resolver.transform);
            }
        }
    }

    private static bool TrySearchForScript<T>(Transform transform, out T searchObject) where T : Component
    {
        if (transform == SceneManager.GetActiveScene().GetRootGameObjects()[0].transform)
        {
            searchObject = null;
            return false;
        }

        Transform t = transform;
        while (t != null)
        {  
            searchObject = t.GetComponent<T>();
            if (searchObject)
            {
                return true;
            }
            else
            {
                t = t.parent;
            }
        }

        searchObject = SceneManager.GetActiveScene().GetRootGameObjects()[0].GetComponent<T>();
        return searchObject;
    }

    public static Delegate CreateCallback<T>(T listener, Type delegateType) where T : EventCallbackData
    {
        return Delegate.CreateDelegate(delegateType, listener.handlerObject, listener.methodName);
    }

    public static T GetSingleAttribute<T>(this MonoBehaviour obj, string methodName, int index = 0) where T : Attribute
    {
        if (obj.GetType().GetMethod(methodName) == null)
        {
            Debug.LogError($"There is no method \'{methodName}\' in class \'{obj.GetType()}\'");
            return null;
        }
        else if (obj.GetType().GetMethod(methodName).GetCustomAttributes(typeof(T), true).Length == 0)
        {
            Debug.LogError($"There is no attribute for method \'{methodName}\' of type \'{typeof(T)}\'");
        }
        return (T)obj.GetType().GetMethod(methodName).GetCustomAttributes(typeof(T), true)[index];
    }
}
