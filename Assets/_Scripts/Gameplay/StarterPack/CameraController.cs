using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float speed = 10f;
    [SerializeField] private Vector3 offset;
    
    private void LateUpdate()
    {
        if (!target)
            return;

        transform.position = Vector3.Lerp(transform.position, target.position + offset, Time.deltaTime * speed);
    }
    
    public void SetTarget(Transform target)
    {
        this.target = target;
    }
}
