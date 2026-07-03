using System.Globalization;
using TMPro;
using UnityEngine;

namespace PuzzleGame.UI
{
    /// <summary>
    /// Presents the current score and moves. It only reflects state pushed by the
    /// "GameManager" through events and never mutates game logic itself.
    /// </summary>
    public class HudView : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text movesText;

        // The art shows the score with a dot as the thousands separator ("1.250").
        // Format explicitly so it doesn't depend on the machine's current culture.
        private static readonly NumberFormatInfo ScoreFormat = new NumberFormatInfo
        {
            NumberGroupSeparator = ".",
            NumberGroupSizes = new[] { 3 }
        };

        private void OnEnable()
        {
            gameManager.ScoreChanged += UpdateScore;
            gameManager.MovesChanged += UpdateMoves;
        }

        private void OnDisable()
        {
            gameManager.ScoreChanged -= UpdateScore;
            gameManager.MovesChanged -= UpdateMoves;
        }

        private void Start()
        {
            // Pull current values in case the manager initialised before we subscribed.
            UpdateScore(gameManager.Score);
            UpdateMoves(gameManager.MovesRemaining);
        }

        private void UpdateScore(int score) => scoreText.text = score.ToString("#,0", ScoreFormat);
        private void UpdateMoves(int moves) => movesText.text = moves.ToString();
    }
}
