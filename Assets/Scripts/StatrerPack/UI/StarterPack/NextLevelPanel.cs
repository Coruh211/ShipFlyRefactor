using System;
using Menus;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevelPanel : MonoBehaviour
{
    [SerializeField] private GameObject DoubleButton;
    [SerializeField] private GameObject NoThanksButton;
    [SerializeField] private GameObject ContinueButton;
    [SerializeField] private bool enableAds = true;

    private void Start()
    {
        if (!enableAds)
            ShowContinue(true);
    }

    public void DoubleClick()
    {
        // TODO remove and open ads instead
        OnWatchedAds();
    }

    public void OnWatchedAds()
    {
        CoinsTakeUIManager.Instance.GetManyCoinsInScreen(transform.GetChild(0).position);
        ShowContinue(true);
    }

    public void NoClick()
    {
        SceneManager.LoadScene(0);
    }

    public void ContinueClick()
    {
        SceneManager.LoadScene(0);
    }

    private void ShowContinue(bool show)
    {
        DoubleButton?.SetActive(!show);
        NoThanksButton?.SetActive(!show);
        ContinueButton?.SetActive(!show);
    }
}