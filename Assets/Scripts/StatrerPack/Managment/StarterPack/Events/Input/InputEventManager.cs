using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Gameplay.Input
{
    public class InputEventManager : EventTrigger
    {
        public bool multitouchEnabled;

        public InputListenersManager Settings;
        public static InputEventManager Instance
        {
            get; protected set;
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        /// <summary>
        /// NOTE: Add listener only on Start
        /// This is how to add listener on Input event
        /// <code>
        /// private void Start()
        /// {
        ///     ViewportDragInput.Instance.AddListener(EventTriggerType.Drag, OnDrag);
        /// }
        /// 
        /// private void OnDrag() {}
        /// </code> 
        /// </summary> 
        /// <param name="type"></param>
        /// <param name="handler">Only non-lambda methods (objects' methods)</param>
        public void AddListener(InputDelegateWrapper wrapper)
        {
            int entryIndex = triggers.FindIndex(trigger => trigger.eventID == wrapper.listener.triggerType);
            if (entryIndex == -1)
            {
                Entry entry = new Entry();
                entry.eventID = wrapper.listener.triggerType;
                entry.callback.AddListener(wrapper.callback);
                triggers.Add(entry);
            }
            else
            {
                triggers[entryIndex].callback.AddListener(wrapper.callback);
            }
        }
        /// <summary>
        /// Parameters' types and requirenments as for AddListener
        /// </summary> 
        /// <param name="type"></param>
        /// <param name="handler">Only non-lambda methods (objects' methods)</param>
        public void RemoveListener(EventTriggerType type, UnityAction<BaseEventData> handler)
        {
            int entryIndex = triggers.FindIndex(trigger => trigger.eventID == type);
            if (entryIndex != -1)
            {
                triggers[entryIndex].callback.RemoveListener(handler);
            }
        }
    }
}
