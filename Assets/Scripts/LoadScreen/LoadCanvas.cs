using System;
using Infrastructure;
using UniRx;
using UnityEngine;

namespace LoadScreen
{
    public class LoadCanvas: MonoBehaviour
    {
        private float hideDuration = 3f;

        private void Awake()
        {
            DontDestroyOnLoad(this);
        }

        public void Show() => 
            gameObject.SetActive(true);

        public void Hide()
        {
            Observable.Timer(hideDuration.sec())
                .Subscribe(x => gameObject.SetActive(false));
        }
            
    }
}