using System;

namespace Services.Input
{
    public interface IInputService
    {
        public event Action OnMouseUp;
        public event Action OnMouseDown;
        public event Action OnHoldTap;
    }
}