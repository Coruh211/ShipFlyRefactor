using Imphenzia;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GradientSky : MonoBehaviour
{
    [SerializeField] private Gradient gradient;
    private GradientSkyCamera gradientSkyCamera;

    private void Awake()
    {
        gradientSkyCamera = GetComponent<GradientSkyCamera>();
        gradientSkyCamera.gradient = gradient;
    }
}
