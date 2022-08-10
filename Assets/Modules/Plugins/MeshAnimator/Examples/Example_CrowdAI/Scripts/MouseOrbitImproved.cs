// http://wiki.unity3d.com/index.php?title=MouseOrbitImproved#Code_C.23
using UnityEngine;
using System.Collections;
using System.Linq;

[AddComponentMenu("Camera-Control/Mouse Orbit with zoom")]
public class MouseOrbitImproved : MonoBehaviour
{

    public Transform target;
    public float scrollMult = 5f;
    public float distance = 5.0f;
    public float xSpeed = 120.0f;
    public float ySpeed = 120.0f;

    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;

    public float distanceMin = .5f;
    public float distanceMax = 15f;

    private PlayerControlled controlled;

    float x = 0.0f;
    float y = 0.0f;
    private bool locked;

    // Use this for initialization
    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
    }

    void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;

        if (Input.GetMouseButtonDown(2))
        {
            locked = !locked;
        }
        if (locked) return;
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (controlled)
            {
                Destroy(controlled);
            }
            else
            {
                var units = FindObjectsOfType<AI_Unit>().ToList();
                units.RemoveAll(x => !x.name.StartsWith("Unit_Human"));
                controlled = units[Random.Range(0, units.Count)].gameObject.AddComponent<PlayerControlled>();
            }
        }

        if (target)
        {
            x += Input.GetAxis("Mouse X") * xSpeed * distance * 0.02f * Time.deltaTime;
            y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f * Time.deltaTime;

            y = ClampAngle(y, yMinLimit, yMaxLimit);

            Quaternion rotation = Quaternion.Euler(y, x, 0);

            distance = Mathf.Lerp(distance, Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * scrollMult, distanceMin, distanceMax), Time.deltaTime * 10f);

            RaycastHit hit;
            if (Physics.Linecast(target.position, transform.position, out hit))
            {
                distance -= hit.distance;
            }
            Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
            Vector3 position = rotation * negDistance + target.position;

            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * 5f);
            transform.position = Vector3.Lerp(transform.position, position, Time.deltaTime * 5f);

            if (!controlled)
            {
                if (Input.GetKey(KeyCode.W))
                {
                    Vector3 p = target.transform.position;
                    p += transform.forward * Time.deltaTime * 50f;
                    p.y = 0;
                    target.transform.position = p;
                }
                if (Input.GetKey(KeyCode.S))
                {
                    Vector3 p = target.transform.position;
                    p -= transform.forward * Time.deltaTime * 50f;
                    p.y = 0;
                    target.transform.position = p;
                }
                if (Input.GetKey(KeyCode.A))
                {
                    Vector3 p = target.transform.position;
                    p += Vector3.Cross(transform.forward, Vector3.up) * Time.deltaTime * 50f;
                    p.y = 0;
                    target.transform.position = p;
                }
                if (Input.GetKey(KeyCode.D))
                {
                    Vector3 p = target.transform.position;
                    p += Vector3.Cross(transform.forward, -Vector3.up) * Time.deltaTime * 50f;
                    p.y = 0;
                    target.transform.position = p;
                }
            }
        }
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}