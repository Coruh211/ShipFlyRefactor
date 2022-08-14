using System;
using UnityEngine;

public class FpsController : MonoBehaviour
{
    [SerializeField] private int targetFps = 60;

    public void OnEnable()
    {
        Application.targetFrameRate = targetFps;
    }

    public void OnDisable()
    {
        Application.targetFrameRate = -1;
    }

    public void ApplyTargetFps()
    {
        Application.targetFrameRate = targetFps;
    }
}