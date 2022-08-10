using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReviveTimer : MonoBehaviour
{
    public Action OnTimerRestart;
    public Action OnTimerResume;
    public Action OnTimerStopped;
    public Action OnTimerEnded;
    
    [SerializeField] private float initialTime;
    [SerializeField] private Image fillImage;
    [SerializeField] private TextMeshProUGUI timeText;

    private float timer;
    private bool isStopped;
    private bool isEnded;

    public bool IsEnded => isEnded;

    private void Start()
    {
        RestartTimer();
    }

    private void Update()
    {
        if (isEnded || isStopped)
            return;

        timer -= Time.deltaTime;
        if (timer > 0)
        {
            fillImage.fillAmount = timer / initialTime;
            timeText.text = CMath.Digits[(int) Mathf.Ceil(timer)];
        }
        else
        {
            fillImage.fillAmount = 0;
            timeText.text = CMath.Digits[0];
            isEnded = true;
            OnTimerEnded?.Invoke();
        }
    }

    public void ResumeTimer()
    {
        isStopped = false;
        OnTimerResume?.Invoke();
    }

    public void StopTimer()
    {
        isStopped = true;
        OnTimerStopped?.Invoke();
    }

    public void RestartTimer()
    {
        OnTimerRestart?.Invoke();
        timer = initialTime;
        fillImage.fillAmount = 1;
        timeText.text = CMath.Digits[(int) Mathf.Ceil(initialTime)];
        isEnded = false;
    }
}