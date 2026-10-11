using UnityEngine;
using UnityEngine.SceneManagement;

namespace Pasit
{
    public class SceneController : MonoBehaviour
    {
        private void Awake()
        {
            // Ensure mouse cursor is always active and visible in menu/UI scenes
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void LoadScene(string sceneName)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Debug.Log("[SceneController] Loading scene: " + sceneName);
            SceneManager.LoadScene(sceneName);
        }

        public void ReplayLastStage()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            string lastStage = PlayerPrefs.GetString("LastStage", "Stage1");
            Debug.Log("[SceneController] Replaying last stage: " + lastStage);
            SceneManager.LoadScene(lastStage);
        }

        public void ExitGame()
        {
            Debug.Log("[SceneController] Exiting game...");
            Application.Quit();
        }
    }
}
