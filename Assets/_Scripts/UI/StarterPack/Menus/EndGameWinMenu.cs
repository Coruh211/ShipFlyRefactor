using System;
using Menus;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class EndGameWinMenu : UIMenu
{
    [SerializeField] private TextMeshProUGUI randomizeText;
    [SerializeField] private int textCount = 3;
    [SerializeField] private bool randomize = true;

    private void OnEnable()
    {
        if(randomize)
            randomizeText.text = ("ui.win" + Random.Range(0, textCount - 1)).Localize();
    }
}