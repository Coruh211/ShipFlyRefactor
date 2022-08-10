using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class FpsCounter : MonoBehaviour
{
    private Text text;

    private void Start()
    {
        text = GetComponent<Text>();
        Observable.Interval(0.1f.sec())
            .TakeUntilDestroy(gameObject)
            .Subscribe(x => text.text = ((int) (1f / Time.unscaledDeltaTime)).ToString());
    }
}