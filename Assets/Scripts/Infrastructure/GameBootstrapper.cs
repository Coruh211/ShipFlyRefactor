using LoadScreen;
using UnityEngine;

namespace Infrastructure
{
    public class GameBootstrapper: MonoBehaviour, ICoroutineRunner
    {
        private Game game;
        
        public LoadCanvas loadCanvas;

        private void Awake()
        {
            game = new Game(this, loadCanvas);
            game.StateMachine.Enter<BootstrapState>();
            
            DontDestroyOnLoad(this);
        }
    }
}