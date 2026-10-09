using UnityEngine;
using UnityEngine.SceneManagement;

namespace Pasit
{
    public class SceneController : MonoBehaviour
    {
        public void LoadScene(string sceneName)
        {
            Debug.Log("[SceneController] Loading scene: " + sceneName);
            SceneManager.LoadScene(sceneName);
        }

        public void ExitGame()
        {
            Debug.Log("[SceneController] Exiting game...");
            Application.Quit();
        }
    }
}
