using System;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class LocalizeText : MonoBehaviour
{
    private TextMeshProUGUI text;
    private string originalText;
    
    private void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        originalText = text.text;
        
        UpdateText();
        LanguageManager.Instance.CurrentLanguage.Subscribe(UpdateText);
    }

    private void UpdateText()
    {
        text.SetLocalizedText(originalText);
    }
}