using LoadScreen;
using UnityEngine;

namespace Infrastructure
{
    public class LoadLevelState: IPayloadedState<string>
    {
        private const string InitialPointTag = "InitialPoint";
        private const string PlayerPath = "Prefabs/Player/Player";
        private const string HUDPath = "Prefabs/UI/HUD";
        
        private readonly GameStateMachine stateMachine;
        private readonly SceneLoader sceneLoader;
        private readonly LoadCanvas loadCanvas;

        public LoadLevelState(GameStateMachine stateMachine, SceneLoader sceneLoader, LoadCanvas loadCanvas)
        {
            this.stateMachine = stateMachine;
            this.sceneLoader = sceneLoader;
            this.loadCanvas = loadCanvas;
        }

        public void Enter(string sceneName)
        {
            loadCanvas.Show();
            sceneLoader.Load(sceneName, OnLoaded);
        }
        
        public void Exit()
        {
            loadCanvas.Hide();    
        }

        private void OnLoaded()
        {
            var initialPoint = GameObject.FindWithTag(InitialPointTag);
            GameObject player = Instantiate(PlayerPath, initialPoint.transform.position);
            Instantiate(HUDPath);
            
            stateMachine.Enter<GameLoopState>();
        }

        private static GameObject Instantiate(string path)
        {
            var prefab = Resources.Load<GameObject>(path);
            return Object.Instantiate(prefab);
        }

        private static GameObject Instantiate(string path, Vector3 at)
        {
            var prefab = Resources.Load<GameObject>(path);
            return Object.Instantiate(prefab, at, Quaternion.identity);
        }
        
        
    }
}