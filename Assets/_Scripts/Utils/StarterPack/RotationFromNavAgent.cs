using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class RotationFromNavAgent : MonoBehaviour
{
    [SerializeField] private GameObject rotationObject;
    private NavMeshAgent agent;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if ((agent.destination - transform.position).magnitude < 0.1f)
            return;
        
        Quaternion dirQ = Quaternion.LookRotation(agent.destination - transform.position);
        Quaternion slerp = Quaternion.Slerp(rotationObject.transform.rotation, dirQ, 15f * Time.fixedDeltaTime);
        rotationObject.transform.rotation = slerp;
    }
}