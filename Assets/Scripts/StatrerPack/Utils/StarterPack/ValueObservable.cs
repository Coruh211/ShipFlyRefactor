using System;

public class ValueObservable<T>
{
    private Action observers;
    private T _value; // не трогай черточку, а то поломаешь
    public T Value
    {
        get => _value;
        set
        {
            _value = value;
            observers?.Invoke();
        }
    }

    public ValueObservable(T value)
    {
        _value = value;
    }

    public void Subscribe(Action onChange)
    {
        observers += onChange;
    }

    public void UnSubscribe(Action onChange)
    {
        observers -= onChange;
    }

    public void Reset()
    {
        observers = null;
    }
}
