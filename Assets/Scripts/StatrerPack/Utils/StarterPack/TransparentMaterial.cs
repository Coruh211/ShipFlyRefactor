using System;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class TransparentMaterial : MonoBehaviour
{
    [SerializeField] private Material transparentMaterial;
    
    private MeshRenderer meshRender;
    private Material[] defaultMaterial;
    private Material[] materials;

    private void Start()
    {
        meshRender = GetComponent<MeshRenderer>();
        materials = meshRender.materials;
        
        defaultMaterial = new Material[meshRender.materials.Length];
        Array.Copy(meshRender.materials, defaultMaterial, meshRender.materials.Length);
    }

    public void SetTransparentMaterial()
    {
        for (int i = 0; i < materials.Length; i++)
            materials[i] = transparentMaterial;

        meshRender.materials = materials;
    }

    public void SetDefaultMaterial()
    {
        for (int i = 0; i < materials.Length; i++)
            materials[i] = defaultMaterial[i];
        
        meshRender.materials = materials;
    }
}
