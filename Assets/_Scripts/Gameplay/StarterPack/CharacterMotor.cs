using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class CharacterMotor : MonoBehaviour
{
    private Vector3 currentSpeed;
    private float DistanceToTarget;
    private Rigidbody rb;
    private CapsuleCollider col;

    public Rigidbody Rigidbody { get { return rb; } }
    public CapsuleCollider Collider { get { return col; } }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();

        Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        Rigidbody.constraints = RigidbodyConstraints.FreezeRotation;

        
        if (Collider.material.name == "Default (Instance)")
        {
            PhysicMaterial pMat = new PhysicMaterial();
            pMat.name = "Frictionless";
            pMat.frictionCombine = PhysicMaterialCombine.Multiply;
            pMat.bounceCombine = PhysicMaterialCombine.Multiply;
            pMat.dynamicFriction = 0.1f;
            pMat.staticFriction = 0.1f;
            pMat.bounciness = 0f;
            Collider.material = pMat;
        }
    }

    public void Move(Vector3 input, float speed, float speedRotation)
    {
        Rigidbody.velocity = input * speed * Time.fixedDeltaTime;

        if (input == Vector3.zero)
            return;
        Quaternion dirQ = Quaternion.LookRotation(input);
        Quaternion slerp = Quaternion.Slerp(transform.rotation, dirQ, speedRotation * Time.fixedDeltaTime);
        Rigidbody.MoveRotation(slerp);
    }

    public bool MoveTo(Vector3 destination, float speed, float speedRotation, float stopDistance)
    {
        Vector3 relativePos = (destination - transform.position);

        DistanceToTarget = relativePos.magnitude;
        if (DistanceToTarget <= stopDistance)
            return true;
        else
            Move(relativePos.normalized, speed, speedRotation);
        return false;
    }
}
