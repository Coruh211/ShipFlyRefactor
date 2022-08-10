using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MovingThingController : Singleton<MovingThingController>
{

    public Action<GameObject> OnTakeThing;
    public Action<GameObject> OnDropThing;
    
    [SerializeField] private LayerMask thingsLayer;
    [SerializeField] private LayerMask movingPlaneLayer;
    
    private Camera camera;
    private GameObject currentThing;

    private void Start()
    {
        camera = GetComponent<Camera>();
    }

    public void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            if (currentThing)
            {
                OnDropThing?.Invoke(currentThing);
                currentThing = null;
            }
        }
        
        if (Input.GetMouseButton(0))
        {
            if (currentThing)
                currentThing.transform.position = GetTouchPosition();
        }
        
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);

            if (!Physics.Raycast(ray, out hit, 1000f, thingsLayer))
                return;
            
            currentThing = hit.transform.gameObject;
            OnTakeThing?.Invoke(currentThing);
        }
    }

    public Vector3 GetTouchPosition()
    {
        RaycastHit hit;
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, 1000, movingPlaneLayer))
            return hit.point;

        return Vector3.zero;
    }
}