using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (!_instance)
            {
                _instance = FindObjectOfType<T>();
                if (!_instance)
                {
                    Debug.LogError($"Singleton of type {typeof(T)} not contains in scene");
                    return null;
                }

                _instance.AwakeSingletone();
            }
            
            return _instance;
        }
    }

    public static bool InstanceIsNotNull => _instance;

    private void Awake()
    {
        if (!_instance)
        {
            _instance = GetComponent<T>();
            AwakeSingletone();
        }
        else if (_instance != this && !_instance.GetComponentInParent<GlobalObject>())
            Debug.LogError($"Dublicated singleton instance {nameof(T)}", this);
    }  

    /// <summary>
    /// Aka default <see cref="Awake"/>
    /// </summary>
    protected virtual void AwakeSingletone() { }
}