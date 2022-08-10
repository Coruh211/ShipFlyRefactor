using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class EventableMonoBehaviour : MonoBehaviour
{
    private IEnumerable<FieldInfo> events;
    
    protected void Awake()
    {
        events = GetType()
            .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(f => f.FieldType.IsGenericType 
                ? f.FieldType.GetGenericTypeDefinition() == typeof(PriorityEvent<>)
                : f.FieldType == typeof(PriorityEvent));

        AwakeEventable();
    }

    protected void OnDestroy()
    {
        foreach (FieldInfo fieldInfo in events)
            ((IDisposable)fieldInfo.GetValue(this)).Dispose();

        OnDestroyEventable();
    }
    
    protected virtual void AwakeEventable() { }
    protected virtual void OnDestroyEventable() { }
}