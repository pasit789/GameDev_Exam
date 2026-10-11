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

            // If player just played Stage 2 and clicks the retry button (which targets Stage1 by default)
            string lastStage = PlayerPrefs.GetString("LastStage", "");
            if (sceneName == "Stage1" && lastStage == "Stage2")
            {
                sceneName = "Stage2";
            }

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
