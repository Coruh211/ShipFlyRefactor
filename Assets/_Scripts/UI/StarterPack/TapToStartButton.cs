using Menus;
using UnityEngine;
using UnityEngine.EventSystems;

public class TapToStartButton : MonoBehaviour, IInputHandler
{
    [SerializeField] private bool onPointerDown = true;
    
    [InputCallback(EventTriggerType.PointerDown)]
    public void OnPointerDown()
    {
        if (!onPointerDown)
            return;

        OnClick();
    }

    private void OnClick()
    {
        GUIManager.Close<StartGameMenu>();
        GUIManager.Open<GameplayMenu>();
        EventManager.OnStartGame.Invoke();

        this.UnsubscribeInput(nameof(OnPointerDown));
    }
}
