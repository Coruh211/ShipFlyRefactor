using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TouchController : MonoBehaviour
{
    [SerializeField] private LayerMask _layerTouchZone;
    [SerializeField] private LayerMask _layerTouchObject;
    [SerializeField] private bool _allowed = false;
    [SerializeField] private bool _lookPointVector = false;

    [SerializeField] private Camera _outRayCamera;

    [Header("Задается при нажатии на объект")]
    [SerializeField] private TouchObject _currentObject;

    public static TouchController Instance;

    public delegate void Action();
    public Action ReleaseEvent;
    public Action TapEvent;
    public Action DragEvent;

    private void Awake()
    {
        Instance = this;

        if(!_outRayCamera)
            _outRayCamera = Camera.main;
    }

	private void OnMouseDrag()
	{
        if (_allowed && _currentObject)
		{
            _currentObject.MouseDrag();
            DragEvent?.Invoke();
		}
    }

	private void OnMouseDown()
	{
        if (!_allowed)
            return;

        MouseButtonDown();
        TapEvent?.Invoke();
    }

    private void OnMouseUp()
    {
        if (_allowed && _currentObject)
        {
            _currentObject.MouseUp();
            _currentObject = null;
            ReleaseEvent?.Invoke();
        }
    }

	private void MouseButtonDown()
    {
        RaycastHit hit;
        Ray ray = _outRayCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, 1000f, _layerTouchObject))
		{
            Debug.DrawLine(ray.origin, hit.point);
            if(hit.collider.gameObject.TryGetComponent(out TouchObject touchObject))
			{
                _currentObject = touchObject;
                _currentObject.MouseDown();
            }
        }
    }

    public Vector3 GetTouchPosition()
    {
        RaycastHit hit;
        Ray ray = _outRayCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, 2000f, _layerTouchZone))
        {
            Debug.DrawLine(ray.origin, hit.point);

            if (_lookPointVector)
                Debug.Log("Vector point:" + hit.point);

            return hit.point;
        }
        return _currentObject.transform.localPosition;
    }

    public void SetAllowed(bool value) => _allowed = value;
}
