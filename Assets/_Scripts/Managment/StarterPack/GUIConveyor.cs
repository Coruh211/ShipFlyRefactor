using System;
using System.Collections.Generic;
using Menus;
using UnityEngine;

public class GUIConveyor : Singleton<GUIConveyor>
{
    private Queue<IUIMenu> conveyor = new Queue<IUIMenu>();
    private IUIMenu currentMenu;
    private Action onConveyorEnd;
    private bool isStarted;

    public bool IsStarted => isStarted;

    public void OnEnable()
    {
        GUIManager.OnCloseMenuHandler += OnMenuClosed;
    }

    public void OnDisable()
    {
        GUIManager.OnCloseMenuHandler -= OnMenuClosed;
    }

    public GUIConveyor StartConveyor(Action onConveyorEnd)
    {
        this.onConveyorEnd = onConveyorEnd;
        conveyor.Clear();
        isStarted = true;
        return this;
    }

    public GUIConveyor AddMenu<T>() where T : UnityEngine.Object, IUIMenu
    {
        if (!isStarted)
        {
            Debug.LogError("You tried to add menu in conveyor, but it didn't start");
            return this;
        }
        
        conveyor.Enqueue(GUIManager.GetUI<T>());
        if (currentMenu == null)
            MoveConveyor();

        return this;
    }

    public void MoveConveyor(bool closeCurrent = true)
    {
        if (!isStarted)
        {
            Debug.LogError("You tried to conveyor, but it didn't start");
            return;
        }
        
        if (closeCurrent && currentMenu != null)
            GUIManager.Close(currentMenu);

        IUIMenu nextMenu = conveyor.Dequeue();
        if (nextMenu != null)
        {
            currentMenu = nextMenu;
            GUIManager.Open(nextMenu);
        }
        else
        {
            onConveyorEnd?.Invoke();
            ResetConveyor();
        }
    }

    public void ResetConveyor()
    {
        isStarted = false;
        currentMenu = null;
    }

    private void OnMenuClosed(IUIMenu menu)
    {
        if (currentMenu != null && menu == currentMenu)
            MoveConveyor(false);
    }
}