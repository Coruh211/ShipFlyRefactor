using System;
using UnityEngine.EventSystems;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class InputCallbackAttribute : Attribute
{
    public EventTriggerType triggerType;
    public InputCallbackAttribute(EventTriggerType triggerType)
    {
        this.triggerType = triggerType;
    }
}
