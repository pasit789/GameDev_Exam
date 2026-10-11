using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace Pasit
{
    public class ScoreDisplay : MonoBehaviour
    {
        public TextMeshProUGUI scoreText;

        private void Awake()
        {
            // Ensure mouse cursor is fully unlocked and visible in Win/GameOver screens
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (scoreText == null)
            {
                scoreText = GetComponent<TextMeshProUGUI>();
            }

            int score = PlayerPrefs.GetInt("LastScore", GameManager.LastScore);
            if (scoreText != null)
            {
                scoreText.text = "Score : " + score.ToString();
            }

            // Wire the Retry/PlayAgain button specifically to the last stage played
            string lastStage = PlayerPrefs.GetString("LastStage", "Stage1");
            var allButtons = FindObjectsByType<Button>();
            foreach (var b in allButtons)
            {
                if (b.name.Contains("PlayAgain") || b.name.Contains("TryAgain"))
                {
                    var tmp = b.GetComponentInChildren<TextMeshProUGUI>();
                    if (tmp != null)
                    {
                        if (lastStage == "Stage2")
                        {
                            tmp.text = tmp.text.Replace("(STAGE 1)", "(STAGE 2)");
                        }
                    }

                    b.onClick.RemoveAllListeners();
                    b.onClick.AddListener(() => {
                        Cursor.lockState = CursorLockMode.None;
                        Cursor.visible = true;
                        SceneManager.LoadScene(lastStage);
                    });
                }
            }
        }

        private void Update()
        {
            // Safeguard: keep cursor free in results screens
            if (Cursor.lockState != CursorLockMode.None || !Cursor.visible)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
}
