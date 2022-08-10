using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventCallbackData
{
    public Attribute attribute;
    public MonoBehaviour handlerObject;
    public string methodName;

    public EventCallbackData(Attribute attribute, IHandler handlerObject, string methodName)
    {
        this.attribute = attribute;
        this.handlerObject = (MonoBehaviour) handlerObject;
        this.methodName = methodName;
    }
}
