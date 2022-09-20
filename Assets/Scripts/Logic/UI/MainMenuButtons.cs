using UnityEngine;
using UnityEngine.UI;

namespace Logic.UI
{
    public class MainMenuButtons: MonoBehaviour
    {
        private const string Initial = "Initial";
        
        [SerializeField] private Button playButton;
        [SerializeField] private Button shopButton;

        private void Start()
        {
            playButton.onClick.AddListener(LoadSceneOnClick);
            shopButton.onClick.AddListener(OpenShopModuleOnClick);
        }

        private void LoadSceneOnClick()
        {
            LoadScene.Instance.Load(Initial);
        }

        private void OpenShopModuleOnClick()
        {
            Debug.Log("Да, ты когда-нибудь его напишешь");
        }
    }
}