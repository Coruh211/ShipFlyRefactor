using System;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LoadScreen
{
    public class LoadDelay: MonoBehaviour
    {
        [SerializeField] private string nextSceneName;
        [SerializeField] private float delayLoad;

        private void Start()
        {
            ActivateDelay();
        }

        private void ActivateDelay()
        {
            Observable.Timer(delayLoad.sec()).TakeUntilDisable(gameObject)
                .Subscribe(x => SceneManager.LoadScene(nextSceneName));
        }
    }
}