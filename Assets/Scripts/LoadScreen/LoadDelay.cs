using System;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LoadScreen
{
    public class LoadDelay: MonoBehaviour
    {
        [SerializeField] private float delayLoad;
        
        private const string NextSceneName = "MainMenu";

        private void Start()
        {
            ActivateDelay();
        }

        private void ActivateDelay()
        {
            Observable.Timer(delayLoad.sec()).TakeUntilDisable(gameObject)
                .Subscribe(x => SceneManager.LoadScene(NextSceneName));
        }
    }
}