using LoadScreen;
using Services.Input;

namespace Infrastructure
{
    public class Game
    {
        public GameStateMachine StateMachine;

        public Game(ICoroutineRunner coroutineRunner, LoadCanvas loadCanvas)
        {
            StateMachine = new GameStateMachine(new SceneLoader(coroutineRunner), loadCanvas);
        }
    }
}