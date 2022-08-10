using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Gameplay.Input
{
    [Serializable]
    public class InputDelegateWrapper
    {
        public UnityAction<BaseEventData> callback;
        public Delegate listenerDelegate;
        public ParameterInfo parameterInfo;

        public InputEventData listener;

        public InputDelegateWrapper(InputEventData listener)
        {
            this.listener = listener;
            ParameterInfo[] parameters = listener.handlerObject.GetType().GetMethod(listener.methodName).GetParameters();

            if (parameters.Length == 0)
            {
                listenerDelegate = (UnityAction)InputExtensions.CreateCallback(listener, typeof(UnityAction));
                callback = DelegateWithNoParamaters;
            }
            else if (parameters.Length == 1)
            {
                if (parameters[0].ParameterType == typeof(BaseEventData))
                {
                    listenerDelegate = (UnityAction<BaseEventData>)InputExtensions.CreateCallback(listener, typeof(UnityAction<BaseEventData>));
                    callback = HandleBaseInput;
                }
                else if (parameters[0].ParameterType == typeof(PointerEventData))
                {
                    listenerDelegate = (UnityAction<PointerEventData>)InputExtensions.CreateCallback(listener, typeof(UnityAction<PointerEventData>));
                    callback = HandlePointerInput;
                }
                parameterInfo = parameters[0];
            }
        }

        public InputDelegateWrapper(Delegate listenerDelegate, bool hasParameters = false)
        {
            this.listenerDelegate = listenerDelegate;
            if (hasParameters)
            {
                if (parameterInfo.ParameterType == typeof(BaseEventData))
                {
                    callback = HandleBaseInput;
                }
                else
                {
                    callback = HandlePointerInput;
                }
            }
            else
            {
                callback = DelegateWithNoParamaters;
            }
        }        

        public bool TryMultitouch(BaseEventData eventData)
        {
            if (InputEventManager.Instance.Settings.MultitouchEnabled)
            {
                return true;
            }
            else
            {
                return UnityEngine.Input.touchCount == 1 || eventData.GetType() == typeof(PointerEventData) && (eventData as PointerEventData).pointerId == -1;
            }
        }


        // Delegates -------------------------------------------------------------------------------------
        public void DelegateWithNoParamaters(BaseEventData eventData)
        {
            if (!listener.handlerObject)
            {
                InputEventManager.Instance.RemoveListener(listener.triggerType, callback);
                return;
            }
            if (!TryMultitouch(eventData) || !listener.handlerObject.enabled)
            {
                return;
            }

            (listenerDelegate as UnityAction).Invoke();
        }

        public void HandleBaseInput(BaseEventData eventData)
        {
            if (!listener.handlerObject)
            {
                InputEventManager.Instance.RemoveListener(listener.triggerType, callback);
                return;
            }
            if (!TryMultitouch(eventData) || !listener.handlerObject.enabled)
            {
                return;
            }

            (listenerDelegate as UnityAction<BaseEventData>).Invoke(eventData);
        }

        public void HandlePointerInput(BaseEventData eventData)
        {
            if (listener.handlerObject == null)
            {
                InputEventManager.Instance.RemoveListener(listener.triggerType, callback);
                return;
            }
            if (!TryMultitouch(eventData) || !listener.handlerObject.enabled)
            {
                return;
            }

            (listenerDelegate as UnityAction<PointerEventData>).Invoke(eventData as PointerEventData);
        }
    }
}
