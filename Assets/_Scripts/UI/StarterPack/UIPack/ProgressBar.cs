using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private TextMeshProUGUI currentLevelText;
    [SerializeField] private TextMeshProUGUI nextLevelText;

    private void Start()
    {
        int currentLevel = LevelManager.Instance.CurrentLevel.Value;
        currentLevelText.text = CMath.Digits[currentLevel];
        nextLevelText.text = CMath.Digits[currentLevel + 1];
    }

    public void SetFill(float value)
    {
        fillImage.fillAmount = value;
    }
}