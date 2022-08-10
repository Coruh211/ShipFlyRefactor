using UnityEngine;

public class LevelManager : Singleton<LevelManager>
{
    public PrefsValue<int> CurrentLevel = new PrefsValue<int>("CurrentLevel", 0);
    
    public void EndLevel(EndGameStatus status)
    {
        EventManager.OnEndGame.Invoke(status);
        CurrentLevel.Value++;
    }
}