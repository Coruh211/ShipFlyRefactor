using System;
using UnityEngine;

namespace Infrastructure
{
    public class GameBootstrapper: MonoBehaviour
    {
        private Game game;

        private void Awake()
        {
            game = new Game();
            
            DontDestroyOnLoad(this);
        }
    }
}