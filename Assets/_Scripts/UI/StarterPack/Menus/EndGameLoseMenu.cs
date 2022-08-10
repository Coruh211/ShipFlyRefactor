using Menus;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGameLoseMenu : UIMenu
{
    public void NextClick()
    {
        SceneManager.LoadScene(0);
    }
}