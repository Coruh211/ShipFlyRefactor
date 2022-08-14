
using System;
using UnityEngine;

public class RaycastAimExampleGun : MonoBehaviour
{
    [SerializeField] private GameObject gun;
    [SerializeField] private float rotationSpeed = 7;

    private void Start()
    {
        RaycastAimInput.Instance.SetGunOffset(gun.transform.localPosition);
    }

    private void Update()
    {
        gun.transform.rotation = Quaternion.Slerp(gun.transform.rotation, RaycastAimInput.Instance.LastRotation, Time.deltaTime * rotationSpeed);
    }
}
