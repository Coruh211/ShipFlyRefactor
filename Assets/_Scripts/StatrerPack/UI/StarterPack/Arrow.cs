using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Arrow : MonoBehaviour
{
    [SerializeField] private Image image;
    
    private Transform target;
    private Transform player;
    private Camera camera;

    public Transform Target => target;
    public Transform Player => player;

    public void Init(Transform target, Transform player,Color color)
    {
        this.target = target;
        this.player = player;
        
        image.color = color;
        camera = Camera.main;

        SetActiveArrow(false);
    }

    public Vector3 ScreenPosition()
    {
        Vector3 direction = Direction();
        float magn = Mathf.Clamp(direction.magnitude, 0, 40);
        direction = direction.normalized * magn;
        return camera.WorldToScreenPoint(player.position + direction);
    }

    public Vector3 Direction()
    {
        return target.position - player.position;
    }

    public void SetActiveArrow(bool value)
    {
        if (transform.gameObject.activeSelf != value)
            transform.gameObject.SetActive(value);
    }

    public void Rotation()
    {
        Vector3 direction = Direction();
        if (direction != Vector3.zero)
        {
            float angle = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
        else
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, 0), Time.deltaTime * 10);
    }
}
