using UnityEngine;

public class RotateObject : MonoBehaviour
{
    [SerializeField] private float rotateSpeed;
    [SerializeField] private Vector3 rotateVector = new Vector3(0,0, 1);

    private void Update() => transform.Rotate(rotateVector * (rotateSpeed * Time.deltaTime));
}
