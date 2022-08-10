using FSG.MeshAnimator;
using UnityEngine;

public class PlayerControlled : MonoBehaviour
{
    private const string AnimIdle = "BreathingIdle";
    private const string AnimForward = "Running";
    private const string AnimBackward = "RunningBackward";
    private const string AnimLeft = "Running";
    private const string AnimRight = "Running";

    private float _moveSpeed = 30f;
    private Transform _origTarget;
    private string _currentAnim;

    private void OnEnable()
    {
        GetComponent<AI_Unit>().enabled = false;
        _origTarget = Camera.main.GetComponent<MouseOrbitImproved>().target;
        Camera.main.GetComponent<MouseOrbitImproved>().target = transform;
    }
    private void OnDisable()
    {
        GetComponent<AI_Unit>().enabled = true;
        if (Camera.main)
            Camera.main.GetComponent<MouseOrbitImproved>().target = _origTarget;
    }
    private void Update()
    {
        var rot = Camera.main.transform.eulerAngles;
        rot.z = 0;
        transform.rotation = Quaternion.Euler(rot);
        Vector3 forwardVector = transform.forward;
        string animationToPlay;
        if (Input.GetKey(KeyCode.W))
        {
            transform.position += forwardVector * Time.deltaTime * _moveSpeed;
            animationToPlay = AnimForward;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            transform.position -= forwardVector * Time.deltaTime * _moveSpeed;
            animationToPlay = AnimBackward;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            transform.position += Vector3.Cross(forwardVector, Vector3.up) * Time.deltaTime * _moveSpeed;
            transform.LookAt(transform.position + Vector3.Cross(forwardVector, Vector3.up));
            animationToPlay = AnimLeft;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            transform.position -= Vector3.Cross(forwardVector, Vector3.up) * Time.deltaTime * _moveSpeed;
            transform.LookAt(transform.position - Vector3.Cross(forwardVector, Vector3.up));
            animationToPlay = AnimRight;
        }
        else
        {
            animationToPlay = AnimIdle;
        }
        var p = transform.position;
        p.y = 0;
        transform.position = p;
        var rotAngle = transform.rotation.eulerAngles;
        rotAngle.x = 0;
        rotAngle.z = 0;
        transform.rotation = Quaternion.Euler(rotAngle);
        if (_currentAnim != animationToPlay)
        {
            GetComponent<MeshAnimatorBase>().Crossfade(animationToPlay, 0.25f);
            _currentAnim = animationToPlay;
        }
    }
}