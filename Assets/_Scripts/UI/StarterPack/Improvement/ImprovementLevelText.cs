using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(TextMeshProUGUI))]
public class ImprovementLevelText : MonoBehaviour
{
    [SerializeField] private bool isFirst;
    
    private TextMeshProUGUI text;
    private PrefsValue<int> improvementLevel;

    private void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        improvementLevel = isFirst ? PurchasesManager.Instance.Improvement0Level : PurchasesManager.Instance.Improvement1Level;

        void UpdateText()
        {
            text.SetLocalizedText("[improvement.level] " + (improvementLevel.Value + 1));
        }

        improvementLevel.GetObservable().Subscribe(UpdateText);
        LanguageManager.Instance.CurrentLanguage.Subscribe(UpdateText);
    }
}