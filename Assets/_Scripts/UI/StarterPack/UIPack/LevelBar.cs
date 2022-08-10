using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelBar : MonoBehaviour
{
    [SerializeField] private GameObject circlePrefab;
    [SerializeField] private Sprite offSprite;
    [SerializeField] private Sprite onSprite;
    [SerializeField] private bool initAtStart = true;
    [SerializeField] private int startMaxValue = 5;

    private RectTransform rectTransform;
    private int value;
    private int maxValue;
    
    public int Value => value;
    public int MaxValue => maxValue;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        if (!initAtStart)
            return;
        
        SetMaxValue(startMaxValue);
        SetValue(LevelManager.Instance.CurrentLevel.Value % maxValue);
    }

    public void SetValue(int value)
    {
        this.value = value;
        
        for (int i = 0; i < maxValue; i++)
        {
            GameObject circle = transform.GetChild(i).gameObject;
            TextMeshProUGUI text = circle.GetComponentInChildren<TextMeshProUGUI>();
            
            if (text)
                text.enabled = value < i;

            Image circleImage = circle.GetComponentInChildren<Image>();
            circleImage.sprite = value > i ? onSprite : offSprite;
            circleImage.SetNativeSize();
        }
    }

    public void SetMaxValue(int maxValue)
    {
        if (maxValue == this.maxValue)
            return;
        
        if (this.maxValue > maxValue)
        {
            for (int i = transform.childCount - 1; i >= maxValue; i--)
                Destroy(transform.GetChild(i).gameObject);
        }
        else
        {
            for (int i = 0; i < maxValue - this.maxValue; i++)
                Instantiate(circlePrefab, transform);
        }

        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 100 * maxValue);
        this.maxValue = maxValue;
    }
}