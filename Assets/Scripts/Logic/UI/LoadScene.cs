using UnityEngine;
using UnityEngine.SceneManagement;

namespace Logic.UI
{
    public class LoadScene: Singleton<LoadScene>
    {
        public void Load(string name)
        {
            SceneManager.LoadScene(name);
        }
    }
}