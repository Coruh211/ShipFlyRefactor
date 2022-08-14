using UnityEngine;
using UnityEngine.UI;

public class ImprovementBuyButton : MonoBehaviour
{
    [SerializeField] private bool isFirst;
    [SerializeField] private Sprite notActiveSprite;
    
    private Sprite normalSprite;
    private Button button;
    private Image image;
    private PrefsValue<int> improvementLevel;
    private ValueObservable<int> improvementPrice;

    private void Start()
    {
        improvementLevel = isFirst ? PurchasesManager.Instance.Improvement0Level : PurchasesManager.Instance.Improvement1Level;
        improvementPrice = isFirst ? PurchasesManager.Instance.Improvement0Price : PurchasesManager.Instance.Improvement1Price;
        button = GetComponent<Button>();
        image = GetComponent<Image>();
        normalSprite = image.sprite;
        UpdateButton();
        CoinsManager.Instance.CoinsCount.GetObservable().Subscribe(UpdateButton);
        improvementPrice.Subscribe(UpdateButton);
        improvementLevel.GetObservable().Subscribe(UpdateButton);
    }

    public void BuyClick()
    {
        improvementLevel.Value++;
        CoinsManager.Instance.CoinsCount.Value -= improvementPrice.Value;
    }

    private void UpdateButton()
    {
        bool canBuy = CoinsManager.Instance.CoinsCount.Value >= improvementPrice.Value;
        button.enabled = canBuy;
        image.sprite = canBuy ? normalSprite : notActiveSprite;
    }
}