using System;
using UnityEngine;

public class RenderUnit : MonoBehaviour
{
    [SerializeField] private Camera renderCamera;
    
    public RenderTexture RenderTexture { get; private set; }
    public GameObject RenderModel { get; private set; }

    public Camera RenderCamera => renderCamera;

    public void Initialize(RenderTexture renderTexture, GameObject renderModel)
    {
        RenderTexture = renderTexture;
        RenderModel = renderModel;
    }

    public void ToggleRender(bool active)
    {
        gameObject.SetActive(active);
    }
}