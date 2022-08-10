using UnityEngine;

public class PurchasesManager : Singleton<PurchasesManager>
{
    public PrefsValue<int> Improvement0Level = new PrefsValue<int>("Improvement0Level", 0);
    public PrefsValue<int> Improvement1Level = new PrefsValue<int>("Improvement1Level", 0);
    public ValueObservable<int> Improvement0Price = new ValueObservable<int>(10);
    public ValueObservable<int> Improvement1Price = new ValueObservable<int>(10);

    private void Start()
    {
        if (!GameManager.Instance.overridesInstaller.Enable)
            return;
        
        if (GameManager.Instance.overridesInstaller.Improvement0Level != -1)
            Improvement0Level.Value = GameManager.Instance.overridesInstaller.Improvement0Level;
        
        if (GameManager.Instance.overridesInstaller.Improvement1Level != -1)
            Improvement1Level.Value = GameManager.Instance.overridesInstaller.Improvement1Level;
    }
}