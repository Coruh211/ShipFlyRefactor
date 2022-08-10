using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

// Для того, чтобы эвент был привязан к компоненту, надо его унаследовать от EventableMonoBehaviour
// Привязанный эвент удаляется вместе с удалением этого компонента автоматически (все его подписчики тоже отвязываются)
public class PriorityEventExample : EventableMonoBehaviour 
{
    // Эвент с двумя и более параметрами надо делать с помощью кортежа
    public PriorityEvent<(int index, string str)> event1 = new PriorityEvent<(int index, string str)>();
    
    // Эвент с одним параметром можно и без кортежа
    private PriorityEvent<int> event2 = new PriorityEvent<int>();
    
    // Эвент без параметров (non-generic type)
    private PriorityEvent event3 = new PriorityEvent();

    private void Start()
    {
        // Подписки на эвент с кортежем
        event1.Subscribe(args => Debug.Log("Called event1 sub1 " + args.index));
        event1.Subscribe(args => Debug.Log("Called event1 sub2 " + args.str), 1);
        event1.Invoke((321, "Some string")); // Вызов и передача аргументов через кортеж
        
        // Подписка на эвент без кортежа
        event2.Subscribe(args => Debug.Log("Called event2 " + args));
        
        // Подписка и вызов эвента без параметров
        event3.Subscribe(() => Debug.Log("Called event3"));
        event3.Invoke();
    }

    // Вместо Awake и OnDestroy надо использовать AwakeEventable и OnDestroyEventable
    protected override void OnDestroyEventable() 
    {
        // Через секунду после уничтожение объекта с эвентами вызываем event2, но вызов подписчиков не произойдет, потому что эвент уничтожен
        Observable.Timer(1.sec()).Subscribe(x => event2.Invoke(123));
    }
}