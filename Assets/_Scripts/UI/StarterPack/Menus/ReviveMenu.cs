using System;
using Menus;
using UnityEngine;

public class ReviveMenu : UIMenu
{
    [SerializeField] private ReviveTimer reviveTimer;

    private void OnEnable()
    {
        reviveTimer.RestartTimer();
    }
}