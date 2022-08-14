using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;

public class AttributeSearchHelper<HandlerType, CustomAttribute> where CustomAttribute : Attribute where HandlerType : IHandler
{
    private const string PARAMETERS_ERROR_MESSAGE = "No parameter of BaseEventData class in method {0} of class {1};";

    public List<T> FindAttributes<T>(List<HandlerType> handlers) where T : EventCallbackData
    {
        List<T> inputEvents = new List<T>();

        foreach (HandlerType handler in handlers)
        {
            inputEvents.AddRange(SearchForListeners<T>(handler));
        }
        return inputEvents;
    }

    public List<T> SearchForListeners<T>(HandlerType handler) where T : EventCallbackData
    {
        List<T> listeners = new List<T>();

        foreach (MethodInfo method in handler.GetType().GetMethods())
        {
            ResolveAllAttributes(method, handler, listeners);
        }
        return listeners;
    }

    public void ResolveAllAttributes<T>(MethodInfo method, HandlerType handler, List<T> listeners) where T : EventCallbackData
    {
        CustomAttribute attribute = (CustomAttribute)method.GetCustomAttribute(typeof(CustomAttribute), true);
        if (attribute != null)
        {
            if (ValidateMethod<BaseEventData>(method))
            {
                listeners.Add((T)Activator.CreateInstance(typeof(T), new object[] { attribute, handler, method.Name }));
            }
            else
            {
                throw new ArgumentException(ParametersErrorMessage(method, handler));
            }
        }
    }

    public bool ValidateMethod<T>(MethodInfo methodInfo)
    {
        if (methodInfo.GetParameters().Length > 1
            || methodInfo.GetParameters().Length == 1
            && methodInfo.GetParameters()[0].ParameterType.BaseType != typeof(T)
            && methodInfo.GetParameters()[0].ParameterType != typeof(T))
        {
            return false;
        }
        return true;
    }

    public string ParametersErrorMessage(MethodInfo methodInfo, IHandler handler)
    {
        return string.Format(PARAMETERS_ERROR_MESSAGE, methodInfo.Name, handler.GetType());
    }
}
