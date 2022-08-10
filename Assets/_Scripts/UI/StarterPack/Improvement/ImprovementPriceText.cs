using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(TextMeshProUGUI))]
public class ImprovementPriceText : MonoBehaviour
{
    [SerializeField] private bool isFirst;
    private TextMeshProUGUI text;
    private ValueObservable<int> improvementPrice;

    private void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        improvementPrice = isFirst ? PurchasesManager.Instance.Improvement0Price : PurchasesManager.Instance.Improvement1Price;
        text.text = improvementPrice.Value.ToString();
        improvementPrice.Subscribe(() => { text.text = improvementPrice.Value.ToString(); });
    }
}