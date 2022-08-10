using UnityEngine;

public class CoinsManager : Singleton<CoinsManager>
{
    public PrefsValue<int> CoinsCount = new PrefsValue<int>("CoinsCount", 0);

    private void Start()
    {
        if (!GameManager.Instance.overridesInstaller.Enable)
            return;
        
        if (GameManager.Instance.overridesInstaller.Coins != -1)
            CoinsCount.Value = GameManager.Instance.overridesInstaller.Coins;
    }

    public void RemoveCoinsCredit(int count)
    {
        CoinsCount.Value -= count;
    }

    public bool TryRemoveCoins(int count)
    {
        if(CoinsCount.Value>=count)
        {
            CoinsCount.Value -= count;
            return true;
        }
        return false;
    }

    public void AddCoins(int count)
    {
        CoinsCount.Value += count;
    }
}
