using UnityEngine;

public class GlobalEvent<T> : PriorityEvent<T>
{
    public GlobalEvent(){}
    
    /// <summary>
    /// Отписывает всех подписчиков
    /// </summary>
    public override void Dispose()
    {
#if UNITY_EDITOR
        string fromClassName = Extensions.NameOfCallingClass();
        if (!ownerClassName.Equals(fromClassName) && !fromClassName.Equals(nameof(EventManager)))
            Debug.LogError($"You dispose event in class {ownerClassName} from class {fromClassName}. This is not allowed, dispose events from the same class");
#endif
        subscribersList.Clear();
    }

    /// <summary>
    /// Вызывает каллбеки у всех подписчиков
    /// </summary>
    public override void Invoke(T args)
    {
        for (int i = 0; i < subscribersList.Count; i++)
            subscribersList[i].Item1.Invoke(args);
    }
}

public class GlobalEvent : PriorityEvent
{
    public GlobalEvent(){}
    
    /// <summary>
    /// Отписывает всех подписчиков
    /// </summary>
    public override void Dispose()
    {
#if UNITY_EDITOR
        string fromClassName = Extensions.NameOfCallingClass();
        if (!ownerClassName.Equals(fromClassName) && !fromClassName.Equals(nameof(EventManager)))
            Debug.LogError($"You dispose event in class {ownerClassName} from class {fromClassName}. This is not allowed, dispose events from the same class");
#endif
        
        subscribersList.Clear();
    }

    /// <summary>
    /// Вызывает каллбеки у всех подписчиков
    /// </summary>
    public override void Invoke()
    {
        for (int i = 0; i < subscribersList.Count; i++)
            subscribersList[i].Item1.Invoke();
    }
}