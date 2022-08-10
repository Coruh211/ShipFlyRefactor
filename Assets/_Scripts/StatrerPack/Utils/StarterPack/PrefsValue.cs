using UnityEngine;

public class PrefsValue<T>
{
    private string key;
    private bool isLoaded;
    private T defaultValue;
    private ValueObservable<T> _value; // не трогай черточку, а то поломаешь
    
    public T Value
    {
        get
        {
            if (!isLoaded)
            {
                isLoaded = true;
                _value.Value = ValueGet();
            }

            return _value.Value;
        }

        set
        {
            _value.Value = value;
            ValueSet(value);
        }
    }

    public PrefsValue(string key, T defaultValue)
    {
        this.key = key;
        this.defaultValue = defaultValue;
        _value = new ValueObservable<T>(defaultValue);
    }
    
    public static PrefsValue<T>[] CreateArray(string name, T defaultValue, int count)
    {
        PrefsValue<T>[] result = new PrefsValue<T>[count];
        for (int i = 0; i < count; i++)
            result[i] = new PrefsValue<T>(name + "_" + i, defaultValue);
        
        return result;
    }

    public ValueObservable<T> GetObservable()
    {
        return _value;
    }

    private void ValueSet(T value)
    {
        switch (value)
        {
            case int i:
                PlayerPrefs.SetInt(key, i);
                break;
            case float f:
                PlayerPrefs.SetFloat(key, f);
                break;
            case string s:
                PlayerPrefs.SetString(key, s);
                break;
            case bool b:
                PlayerPrefs.SetInt(key, b ? 1 : 0);
                break;
        }
    }
    
    private T ValueGet()
    {
        if (typeof(T) == typeof(int))
            return (T)(object) PlayerPrefs.GetInt(key, (int)(object) defaultValue);
        if (typeof(T) == typeof(float))
            return (T)(object) PlayerPrefs.GetFloat(key, (float)(object) defaultValue);
        if (typeof(T) == typeof(string))
            return (T) (object) PlayerPrefs.GetString(key, (string)(object) defaultValue);
        if (typeof(T) == typeof(bool))
            return (T) (object) (PlayerPrefs.GetInt(key, (bool)(object) defaultValue ? 1 : 0) == 1);
        
        return _value.Value;
    }
}