using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class ScreenResolution : MonoBehaviour
{
    private static bool isSetted;

    private int frameCount;
    private float deltaTimeSum;
    private static IDisposable checkTimer;
    private static IDisposable sampleTimer;

    void Start()
    {

#if UNITY_IOS
        return;
#endif

        if (isSetted)
            return;
        
        try
        {
            bool isAndroid;
#if UNITY_IOS || UNITY_IPHONE
            isAndroid = false;
#else
            isAndroid = true;
#endif
            
            if (isAndroid && getSDKInt() <= 27 || PlayerPrefs.HasKey("LowRes"))
                SetResolution(1280);
            else if (checkTimer == null)
            {
                sampleTimer = Observable.EveryUpdate().Subscribe(x =>
                {
                    deltaTimeSum += Time.deltaTime;
                    frameCount++;
                });
                
                checkTimer = Observable.Timer(TimeSpan.FromSeconds(3f)).Subscribe(x =>
                {
                    float avgFrameRate = frameCount / deltaTimeSum;
                    if (avgFrameRate < 40)
                    {
                        SetResolution(1280);
                        PlayerPrefs.SetInt("LowRes", 1);
                        sampleTimer.Dispose();
                        checkTimer.Dispose();
                    }
                });
            }
        }
        catch (Exception e) { }
    }
    
    private void SetResolution(int width)
    {
        int EndScreenResolution = Mathf.CeilToInt(Display.main.renderingWidth / (float) Display.main.renderingHeight * width);
        Display.main.SetRenderingResolution(EndScreenResolution, width);
        isSetted = true;
    }
    
    private int getSDKInt() {
        using (var version = new AndroidJavaClass("android.os.Build$VERSION")) {
            return version.GetStatic<int>("SDK_INT");
        }
    }
}