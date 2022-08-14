using System;
using System.Collections.Generic;
using System.Linq;
using PathologicalGames;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class CoinsTakeUIManager : Singleton<CoinsTakeUIManager>
{
    [SerializeField] private CoinMover coinPrefab;
    [SerializeField] private RectTransform toPoint;
    [SerializeField] private RectTransform parent;
    [SerializeField] private CanvasScaler canvasScaler;

    public void OnTakeOneCoinInWorld(Vector3 worldPosition)
    {
        Vector2 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
        screenPosition /= new Vector2(Screen.width / canvasScaler.referenceResolution.x, Screen.height / canvasScaler.referenceResolution.y);
        
        CoinMover coin = PoolManager.Pools["CoinsPool"].Spawn(coinPrefab.gameObject).GetComponent<CoinMover>();
        coin.gameObject.SetActive(true);
        coin.GetComponent<RectTransform>().anchoredPosition = screenPosition;
        coin.MoveAt(toPoint.position, 1);
    }

    public void GetManyCoinsInScreen(Vector2 spawnPosition, int countReward = 0, int count = 10, float radius = 150)
    {
        int rewardCoin = countReward / count;
        for (int i = 0; i < count; i++)
        {
            CoinMover coin = PoolManager.Pools["CoinsPool"].Spawn(coinPrefab.gameObject).GetComponent<CoinMover>();
            coin.transform.SetParent(parent);
            coin.gameObject.SetActive(true);
            coin.GetComponent<RectTransform>().anchoredPosition = new Vector2(Random.Range(-radius, radius), Random.Range(-radius, radius)) + spawnPosition;
            coin.MoveAt(toPoint.position, i == count-1 ? countReward - (count - 1) * rewardCoin : rewardCoin);
        }
    }
}