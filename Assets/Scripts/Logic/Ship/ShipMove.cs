using Services.Input;
using UnityEngine;

namespace Logic.Ship
{
    public class ShipMove: MonoBehaviour
    {
        [SerializeField] private float speed;
        
        private IInputService inputService;

        private void Init(IInputService inputService)
        {
            this.inputService = inputService;
            inputService.OnHoldTap += MoveUpShip;
        }

        private void OnDisable()
        {
            inputService.OnHoldTap -= MoveUpShip;
        }

        private void MoveUpShip()
        {
            var transformPosition = transform.position;
            transformPosition.y += speed * Time.deltaTime;
            transform.position = transformPosition;
        }
    }
}