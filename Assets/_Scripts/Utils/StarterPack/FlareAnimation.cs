using System;
using DG.Tweening;
using UniRx;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class FlareAnimation : MonoBehaviour
{
    [SerializeField] private float startPosX;
    [SerializeField] private float endPosX;
    [SerializeField] private float animationTime;
    [SerializeField] private float animationDelayTime;

    private Tween tween;
    
    private void Start()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(startPosX, rectTransform.anchoredPosition.y);
        tween = rectTransform.DOAnchorPosX(endPosX, animationTime)
            .SetRecyclable(true)
            .SetEase(Ease.Linear)
            .SetAutoKill(false)
            .OnComplete(() => Observable.Timer(animationDelayTime.sec()).TakeUntilDestroy(this).Subscribe(x =>
            {
                rectTransform.anchoredPosition = new Vector2(startPosX, rectTransform.anchoredPosition.y);
                tween.Restart();
            }));
    }
}