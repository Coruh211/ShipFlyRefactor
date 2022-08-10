using System;
using UnityEngine.EventSystems;

internal class ConditionAttribute : Attribute
{
    public ConditionType condition;

    public ConditionAttribute(ConditionType condition)
    {
        this.condition = condition;
    }
}

public enum ConditionType
{
    Done
}