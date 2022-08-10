using System;
using System.Collections.Generic;
using UnityEngine;

public class RagdollHelper : MonoBehaviour
{
    [SerializeField] private GameObject staticModel;
    [SerializeField] private GameObject ragdollModel;
    [SerializeField] private Transform ragdollMainBone;
    
    private List<(Transform, Transform)> conformity = new List<(Transform, Transform)>();
    private Rigidbody[] ragdollRigidbody;

    public GameObject StaticModel => staticModel;
    public GameObject RagdollModel => ragdollModel;

    private void Start()
    {
        staticModel.transform.RecursiveIterateChildes(child =>
        {
            Transform ragdollConformity = ragdollModel.transform.RecursiveFindChild(child.name);
            if (ragdollConformity != null)
                conformity.Add((child, ragdollConformity));
        });

        ragdollRigidbody = ragdollModel.GetComponentsInChildren<Rigidbody>();
    }

    public void ActivateRagdoll()
    {
        staticModel.SetActive(false);
        ragdollModel.SetActive(true);
        
        foreach (Rigidbody rigidbody in ragdollRigidbody)
            rigidbody.velocity = Vector3.zero;

        ragdollModel.transform.position = staticModel.transform.position;
        foreach ((Transform, Transform) tuple in conformity)
        {
            tuple.Item2.transform.position = tuple.Item1.transform.position;
            tuple.Item2.transform.rotation = tuple.Item1.transform.rotation;
        }
    }

    public void DeactivateRagdoll()
    {
        ragdollModel.SetActive(false);
        staticModel.SetActive(true);
        
        staticModel.transform.position = ragdollMainBone.position.ToVector2().ToVector3Z(staticModel.transform.position.y);
    }
    
    public void FreezeRagdoll(bool freeze)
    {
        foreach (Rigidbody rigidbody in ragdollRigidbody)
            rigidbody.isKinematic = freeze;
    }
}