using System;
using System.Collections.Generic;
using UnityEngine;

public class PriorityEvent<T> : IDisposable
{
    protected List<(Action<T>, int)> subscribersList = new List<(Action<T>, int)>();
    protected string ownerClassName;

    public PriorityEvent()
    {
#if UNITY_EDITOR
        ownerClassName = Extensions.NameOfCallingClass();
#endif
    }
    
    /// <param name="callback">Метод, вызывающийся при срабатывании эвента. Рекомендуется в качестве параметра T использовать кортеж</param>
    /// <param name="priority">Приоритет вызова каллбека. Подписчики с более большим числом вызываются раньше остальных</param>
    public void Subscribe(Action<T> callback, int priority = 0)
    {
        (Action<T>, int) subscriber = (callback, priority);
        for (int i = 0; i < subscribersList.Count; i++)
        {
            int currentPriority = subscribersList[i].Item2;
            if (currentPriority < priority)
            {
                subscribersList.Insert(i, subscriber);
                return;
            }
        }
        
        subscribersList.Add(subscriber);
    }
    
    /// <param name="callback">Подписанный прежде метод</param>
    public void Unsubscribe(Action<T> callback)
    {
        subscribersList.RemoveAll(pair => pair.Item1 == callback);
    }

    /// <summary>
    /// Отписывает всех подписчиков. Не используйте этот метод вне класса, где определен эвент
    /// </summary>
    public virtual void Dispose()
    {
#if UNITY_EDITOR
        string fromClassName = Extensions.NameOfCallingClass();
        if (!ownerClassName.Equals(fromClassName) && !fromClassName.Equals(nameof(EventableMonoBehaviour)))
            Debug.LogError($"You dispose event in class {ownerClassName} from class {fromClassName}. This is not allowed, dispose events from the same class");
#endif
        subscribersList.Clear();
    }

    /// <summary>
    /// Вызывает каллбеки у всех подписчиков. Не используйте этот метод вне класса, где определен эвент
    /// </summary>
    public virtual void Invoke(T args)
    {
#if UNITY_EDITOR
        string fromClassName = Extensions.NameOfCallingClass();
        if (!ownerClassName.Equals(fromClassName) && !fromClassName.Equals(nameof(EventableMonoBehaviour)))
            Debug.LogError($"You call event in class {ownerClassName} from class {fromClassName}. This is not allowed, call events from the same class");
#endif
        for (int i = 0; i < subscribersList.Count; i++)
            subscribersList[i].Item1.Invoke(args);
    }
}

public class PriorityEvent : IDisposable
{
    protected List<(Action, int)> subscribersList = new List<(Action, int)>();
    protected string ownerClassName;

    public PriorityEvent()
    {
#if UNITY_EDITOR
        ownerClassName = Extensions.NameOfCallingClass();
#endif
    }
    
    /// <param name="callback">Метод, вызывающийся при срабатывании эвента. Рекомендуется в качестве параметра T использовать кортеж</param>
    /// <param name="priority">Приоритет вызова каллбека. Подписчики с более большим числом вызываются раньше остальных</param>
    public void Subscribe(Action callback, int priority = 0)
    {
        (Action, int) subscriber = (callback, priority);
        for (int i = 0; i < subscribersList.Count; i++)
        {
            int currentPriority = subscribersList[i].Item2;
            if (currentPriority < priority)
            {
                subscribersList.Insert(i, subscriber);
                return;
            }
        }
        
        subscribersList.Add(subscriber);
    }
    
    /// <param name="callback">Подписанный прежде метод</param>
    public void Unsubscribe(Action callback)
    {
        subscribersList.RemoveAll(pair => pair.Item1 == callback);
    }

    /// <summary>
    /// Отписывает всех подписчиков. Не используйте этот метод вне класса, где определен эвент
    /// </summary>
    public virtual void Dispose()
    {
#if UNITY_EDITOR
        string fromClassName = Extensions.NameOfCallingClass();
        if (!ownerClassName.Equals(fromClassName) && !fromClassName.Equals(nameof(EventableMonoBehaviour)))
            Debug.LogError($"You dispose event in class {ownerClassName} from class {fromClassName}. This is not allowed, dispose events from the same class");
#endif
        subscribersList.Clear();
    }

    /// <summary>
    /// Вызывает каллбеки у всех подписчиков. Не используйте этот метод вне класса, где определен эвент
    /// </summary>
    public virtual void Invoke()
    {
#if UNITY_EDITOR
        string fromClassName = Extensions.NameOfCallingClass();
        if (!ownerClassName.Equals(fromClassName) && !fromClassName.Equals(nameof(EventableMonoBehaviour)))
            Debug.LogError($"You call event in class {ownerClassName} from class {fromClassName}. This is not allowed, call events from the same class");
#endif
        for (int i = 0; i < subscribersList.Count; i++)
            subscribersList[i].Item1.Invoke();
    }
}