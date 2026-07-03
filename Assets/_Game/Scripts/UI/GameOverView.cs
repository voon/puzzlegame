using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame.UI
{
    /// <summary>
    /// Shows / hides the Game Over screen and wires its Replay button.
    /// </summary>
    public class GameOverView : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;

        [Tooltip("Root of the Game Over screen (dim overlay + panel). Toggled active/inactive.")]
        [SerializeField] private GameObject root;

        [SerializeField] private Button replayButton;

        private void OnEnable()
        {
            gameManager.GameOver += Show;
            gameManager.GameStarted += Hide;
            replayButton.onClick.AddListener(gameManager.Replay);
        }

        private void OnDisable()
        {
            gameManager.GameOver -= Show;
            gameManager.GameStarted -= Hide;
            replayButton.onClick.RemoveListener(gameManager.Replay);
        }

        // GameStarted fires on the initial start and on every Replay, so the screen
        // is hidden at boot and re-hidden whenever the game resets.
        private void Show() => root.SetActive(true);
        private void Hide() => root.SetActive(false);
    }
}
