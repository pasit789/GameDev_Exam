using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace Pasit
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;
        public static int LastScore = 0;

        public float timeRemaining = 30f;
        public int currentScore = 0;
        public bool isGameOver = false;

        public TextMeshProUGUI timeText;
        public TextMeshProUGUI scoreText;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            if (timeText == null || scoreText == null)
            {
                var texts = FindObjectsByType<TextMeshProUGUI>();
                foreach (var t in texts)
                {
                    if (t.gameObject.name.Contains("Time") && timeText == null) timeText = t;
                    if (t.gameObject.name.Contains("Score") && scoreText == null) scoreText = t;
                }
            }
            UpdateUI();
        }

        private void Update()
        {
            if (isGameOver) return;

            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                UpdateUI();
            }
            else
            {
                timeRemaining = 0;
                UpdateUI();
                CheckWinLoseCondition();
            }
        }

        public void AddTime(float amount)
        {
            if (isGameOver) return;
            timeRemaining += amount;
            if (timeRemaining < 0) timeRemaining = 0;
            UpdateUI();
        }

        public void AddScore(int amount)
        {
            if (isGameOver) return;
            currentScore += amount;
            if (currentScore < 0) currentScore = 0;
            UpdateUI();
        }

        public void GameOverFromFall()
        {
            if (isGameOver) return;
            isGameOver = true;
            LastScore = currentScore;
            PlayerPrefs.SetInt("LastScore", currentScore);
            SceneManager.LoadScene("GameOver");
        }

        private void CheckWinLoseCondition()
        {
            isGameOver = true;
            LastScore = currentScore;
            PlayerPrefs.SetInt("LastScore", currentScore);

            // Winning condition: score > 10000 within 30 seconds
            if (currentScore > 10000)
            {
                SceneManager.LoadScene("Win");
            }
            else
            {
                SceneManager.LoadScene("GameOver");
            }
        }

        private void UpdateUI()
        {
            if (timeText != null) timeText.text = "Time: " + Mathf.Ceil(timeRemaining).ToString("0");
            if (scoreText != null) scoreText.text = "Score: " + currentScore.ToString();
        }
    }
}
