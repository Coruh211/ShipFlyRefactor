using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class MoneyText : MonoBehaviour
{
    private TextMeshProUGUI text;

    private void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        text.text = CoinsManager.Instance.CoinsCount.Value.ToString();
        CoinsManager.Instance.CoinsCount.GetObservable().Subscribe(() => { text.text = CoinsManager.Instance.CoinsCount.Value.ToString(); });
    }
}