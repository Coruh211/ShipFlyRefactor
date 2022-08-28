using System;

namespace Services.Input
{
    public class InputService : IInputService
    {
        public event Action OnMouseUp;
        public event Action OnMouseDown;
        public event Action OnHoldTap;

        public void Update()
        {
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                OnMouseDown?.Invoke();
            }
            else if(UnityEngine.Input.GetMouseButtonUp(0))
            {
                OnMouseUp?.Invoke();
            }
            else if (UnityEngine.Input.GetMouseButton(0))
            {
                OnHoldTap?.Invoke();   
            }
        }
    }
}