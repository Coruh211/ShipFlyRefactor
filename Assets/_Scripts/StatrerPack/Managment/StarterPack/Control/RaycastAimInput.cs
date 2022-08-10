using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class RaycastAimInput : Singleton<RaycastAimInput>, IDragHandler, IPointerDownHandler
{
    public Action<Quaternion, Vector3> OnAim; // gun rotation, aim position

    [SerializeField] private LayerMask aimLayerMask;
    [SerializeField] private float raycastMaxDistance;
    
    [Tooltip("Отступ рейкаста от центра нажатия в пикселях. Ставьте 0, если хотите чтобы рейкаст шел ровно из места нажатия, а не со смещением (Z всегда должно быть 0)")]
    [SerializeField] private Vector3 raycastPixelsOffset;

    private Camera camera;
    private Vector3 gunOffset;

    public Quaternion LastRotation { get; private set; }
    public Vector3 LastHit { get; private set; }

    private void Start()
    {
        camera = Camera.main;
    }
    
    public void SetGunOffset(Vector3 gunOffset)
    {
        this.gunOffset = gunOffset;
    }

    public void OnDrag(PointerEventData eventData)
    {
        HandleAim();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        HandleAim();
    }

    private void HandleAim()
    {
        Ray ray = camera.ScreenPointToRay(Input.mousePosition.ChangeZ() + raycastPixelsOffset);
        Physics.Raycast(ray, out RaycastHit hit, raycastMaxDistance, aimLayerMask);
        
        Vector3 cameraPosition = camera.transform.position;
        LastHit = hit.distance == 0 ? ray.direction * raycastMaxDistance + cameraPosition : hit.point;
        LastRotation = Quaternion.LookRotation((LastHit - cameraPosition - gunOffset).normalized, Vector3.up);
        
        OnAim?.Invoke(LastRotation, LastHit);
    }
}