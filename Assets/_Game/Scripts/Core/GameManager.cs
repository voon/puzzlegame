using System;
using UnityEngine;

namespace PuzzleGame
{
    /// <summary>
    /// Owns the game state (score / moves) and the game flow (start, move, game over, replay).
    /// It exposes plain C# events so the presentation layer (and, in Task 3, the grid) can react
    /// without this class knowing anything about them. This keeps game logic fully decoupled from
    /// the UI and makes the flow easy to unit-test.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("Rules")]
        [Tooltip("Moves the player starts with.")]
        [SerializeField] private int startingMoves = 5;

        public int Score { get; private set; }
        public int MovesRemaining { get; private set; }
        public bool IsGameOver { get; private set; }

        // True while the player is still allowed to make moves.
        public bool CanPlay => !IsGameOver;

        // Value events carry the new value so listeners don't have to read state back.
        public event Action<int> ScoreChanged;
        public event Action<int> MovesChanged;
        public event Action GameStarted;
        public event Action GameOver;

        private void Start() => StartGame();

        // Resets everything to the initial state and notifies listeners.
        public void StartGame()
        {
            Score = 0;
            MovesRemaining = startingMoves;
            IsGameOver = false;

            // Announce the reset first (hides Game Over), then push the fresh values to the HUD.
            GameStarted?.Invoke();
            ScoreChanged?.Invoke(Score);
            MovesChanged?.Invoke(MovesRemaining);
        }

        // Hooked to the Replay button.
        public void Replay() => StartGame();

        // Adds points to the score. Called with the number of blocks collected in a tap.
        public void AddScore(int points)
        {
            if (IsGameOver || points <= 0) return;
            Score += points;
            ScoreChanged?.Invoke(Score);
        }

        // Spends one move and triggers Game Over when none remain.
        public void ConsumeMove()
        {
            if (IsGameOver) return;

            MovesRemaining = Mathf.Max(0, MovesRemaining - 1);
            MovesChanged?.Invoke(MovesRemaining);

            if (MovesRemaining == 0)
            {
                IsGameOver = true;
                GameOver?.Invoke();
            }
        }
    }
}
