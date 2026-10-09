using UnityEngine;
using TMPro;

namespace Pasit
{
    public class ScoreDisplay : MonoBehaviour
    {
        public TextMeshProUGUI scoreText;

        private void Start()
        {
            if (scoreText == null)
            {
                scoreText = GetComponent<TextMeshProUGUI>();
            }

            int score = PlayerPrefs.GetInt("LastScore", GameManager.LastScore);
            if (scoreText != null)
            {
                scoreText.text = "Score : " + score.ToString();
            }
        }
    }
}
