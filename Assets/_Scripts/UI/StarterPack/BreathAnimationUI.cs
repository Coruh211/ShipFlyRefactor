using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreathAnimationUI : MonoBehaviour
{
    [SerializeField] private AnimationCurve curve;
    
    private RectTransform transform;
    private float currentTime, totalTime;

    private void Start()
    {
        transform = GetComponent<RectTransform>();
        totalTime = curve.keys[curve.keys.Length - 1].time;
    }

    private void Update()
    {
        transform.localScale = new Vector3(curve.Evaluate(currentTime), curve.Evaluate(currentTime), 0);
        currentTime += Time.deltaTime;

        if (currentTime >= totalTime)
            currentTime = 0;
    }
}
