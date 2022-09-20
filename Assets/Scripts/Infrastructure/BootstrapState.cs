using System;

namespace Infrastructure
{
    public class BootstrapState: IState
    {
        private const string Initial = "Initial";
        private readonly GameStateMachine stateMachine;
        private SceneLoader sceneLoader;

        public BootstrapState(GameStateMachine gameStateMachine, SceneLoader sceneLoader)
        {
            stateMachine = gameStateMachine;
            this.sceneLoader = sceneLoader;
        }

        public void Enter()
        {
            RegisterServices();
            sceneLoader.Load(Initial, onLoaded: EnterLoadLevel);
        }

        private void EnterLoadLevel()
        {
            stateMachine.Enter<LoadLevelState, string>("Game");
        }

        private void RegisterServices()
        {
            //throw new NotImplementedException();
        }

        public void Exit()
        {
            //throw new NotImplementedException();
        }
    }
}