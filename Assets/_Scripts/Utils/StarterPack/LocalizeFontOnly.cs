using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class LocalizeFontOnly : MonoBehaviour
{
    private TextMeshProUGUI text;
    
    private void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
		text.font = LanguageManager.Instance.CurrentLanguage.Value.Font;
        LanguageManager.Instance.CurrentLanguage.Subscribe(() =>
        {
            text.font = LanguageManager.Instance.CurrentLanguage.Value.Font;
        });
    }
}