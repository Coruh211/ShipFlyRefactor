using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class LevelText : MonoBehaviour
{
    private TextMeshProUGUI text;

    private void Start()
    {
        text = GetComponent<TextMeshProUGUI>();

        void UpdateText()
        {
            text.SetLocalizedText("[ui.level] " + (LevelManager.Instance.CurrentLevel.Value + 1));
        }
        
        UpdateText();
        LanguageManager.Instance.CurrentLanguage.Subscribe(UpdateText);
    }
}