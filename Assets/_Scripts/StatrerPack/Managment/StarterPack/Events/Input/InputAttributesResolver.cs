using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Input
{
    public class InputAttributesResolver : MonoBehaviour
    {
        [SerializeField] public List<InputEventData> listeners = new List<InputEventData>();
        protected List<InputDelegateWrapper> wrappers = new List<InputDelegateWrapper>();

        protected void Start()
        {
            Subscribe();
        }

        public bool RemoveSubscription(MonoBehaviour obj, string methodName)
        {
            InputCallbackAttribute attribute = obj.GetSingleAttribute<InputCallbackAttribute>(methodName);

            foreach (InputDelegateWrapper wrapper in wrappers)
            {
                if (wrapper.listener.triggerType == attribute.triggerType && wrapper.listener.methodName == methodName && wrapper.listener.handlerObject == obj)
                {
                    InputEventManager.Instance.RemoveListener(wrapper.listener.triggerType, wrapper.callback);
                    return true;
                }
            }         
            return false;
        }

        public void Subscribe()
        {
            for (int i = listeners.Count - 1; i >= 0; i--)
            {
                InputEventData listener = listeners[i];
                InputDelegateWrapper wrapper = WrapListener(listener);
                if (wrapper != null && listener.handlerObject)
                {
                    if (listener.removeThisListener)
                    {
                        InputEventManager.Instance.RemoveListener(listener.triggerType, wrapper.callback);
                    }
                    else
                    {
                        InputEventManager.Instance.AddListener(wrapper);
                    }
                }
            }
        }

        protected InputDelegateWrapper WrapListener(InputEventData listener)
        {
            if (listener.handlerObject == null || !MethodExists(listener))
            {
                listeners.Remove(listener);
                return null;
            }

            InputDelegateWrapper wrapper = new InputDelegateWrapper(listener);
            wrappers.Add(wrapper);
            return wrapper;
        }

        public static bool MethodExists(InputEventData listener) => listener.handlerObject.GetType().GetMethod(listener.methodName) != null;
       
    }
}
