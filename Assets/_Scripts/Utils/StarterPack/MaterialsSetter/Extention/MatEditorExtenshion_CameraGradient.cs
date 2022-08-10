using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MatEditor;
using Imphenzia;

public class MatEditorExtenshion_CameraGradient : MonoBehaviour
{
    [SerializeField] private List<Gradient> gradientPatterns;
    [SerializeField] GradientSkyCamera gradCam;

    private void Awake()
    {
        MaterialSelector.Self.OnSelectedIndexChangeHandler += ChangeGrad;
        ChangeGrad(MaterialSelector.SelectedIndex);
    }

    private void ChangeGrad(int selIndex)
    {
        if (selIndex >= gradientPatterns.Count) return;

        gradCam.gradient = gradientPatterns[selIndex];
        gradCam.cacheGradient = gradientPatterns[selIndex];
    }
}
