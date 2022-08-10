using System;
using UnityEngine.EventSystems;

[Serializable]
public class InputEventData : EventCallbackData
{
    public EventTriggerType triggerType;
    /// <summary>
    /// True when parent prefabs (or prefabs that has other prefabs with input attributes) handle subscribes of outermost (nested) prefabs
    /// </summary>
    public bool removeThisListener; 
    public InputEventData(Attribute attribute, IHandler handlerObject, string methodName) : base(attribute, handlerObject, methodName)
    {
        triggerType = (attribute as InputCallbackAttribute).triggerType;
    }
}
