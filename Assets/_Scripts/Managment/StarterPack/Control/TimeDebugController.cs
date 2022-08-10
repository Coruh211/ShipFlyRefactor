using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class TimeDebugController : MonoBehaviour
{
    [SerializeField] private Text text;
    [SerializeField] private float maxTime = 10f;
    [SerializeField] private float speed = 0.01f;

    private bool leftPushed;
    private bool rightPushed;
    private IDisposable leftTimer;
    private IDisposable rightTimer;

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
            Time.timeScale = 1;

        if (Input.GetKeyDown(KeyCode.DownArrow))
            Time.timeScale = 0;
        
        if (Input.GetKey(KeyCode.LeftArrow))
            Time.timeScale = speed < Time.timeScale ? Time.timeScale - speed : 0;
        
        if (Input.GetKey(KeyCode.RightArrow))
            Time.timeScale += speed;

        if (Time.timeScale > maxTime)
            Time.timeScale = maxTime;
        else if (Time.timeScale < 0)
            Time.timeScale = 0;

        text.text = Time.timeScale.ToString(CultureInfo.CurrentCulture);
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            text.enabled = true;
            leftPushed = true;
            leftTimer?.Dispose();
            rightTimer?.Dispose();
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            text.enabled = true;
            rightPushed = true;
            leftTimer?.Dispose();
            rightTimer?.Dispose();
        }

        if (Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.DownArrow))
        {
            leftPushed = false;
            leftTimer = Observable.Timer(TimeSpan.FromSeconds(1f)).Subscribe(x =>
            {
                if (!rightPushed)
                    text.enabled = false;
            });
        }

        if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            rightPushed = false;
            rightTimer = Observable.Timer(TimeSpan.FromSeconds(1f)).Subscribe(x =>
            {
                if (!leftPushed)
                    text.enabled = false;
            });
        }
    }
#endif
}
