using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame
{
    // Bridges the pure GridModel with the scene: it builds the cell views once, forwards taps
    // to the model, and drives the collect -> wait -> refill sequence. It talks to the GameManager
    // only through AddScore / ConsumeMove / CanPlay, so scoring and game-over logic stay in one place.
    public class BoardController : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private GameManager gameManager;

        [Tooltip("Empty RectTransform that hosts the cells. They are positioned by code, centered on it.")]
        [SerializeField] private RectTransform blocksParent;

        [Tooltip("One sprite per colour, indexed 0..N-1 (block_0 .. block_4).")]
        [SerializeField] private Sprite[] blockSprites;

        [Header("Grid")]
        [SerializeField] private int rows = 6;
        [SerializeField] private int cols = 5;

        [Header("Layout")]
        [Tooltip("On-screen size of a block, in canvas units (sprite is 128x128).")]
        [SerializeField] private Vector2 blockSize = new Vector2(128f, 128f);

        [Tooltip("Fraction of the block height taken by the top 'stud' band (16/128 in the art). " +
                 "Rows overlap by exactly this, so a block hides the studs of the one below and only " +
                 "the top row shows them. Expressed as a ratio so it stays correct if blockSize changes.")]
        [Range(0f, 0.5f)]
        [SerializeField] private float topStudRatio = 16f / 128f;

        [Tooltip("Horizontal gap between columns, in canvas units. 0 = flush (matches the preview).")]
        [SerializeField] private float horizontalSpacing = 0f;

        [Header("Timing")]
        [Tooltip("Seconds to wait between collecting blocks and refilling the grid.")]
        [SerializeField] private float refillDelay = 1f;

        private GridModel _model;
        private BlockView[,] _views;
        private bool _busy; // true while waiting to refill; blocks input meanwhile

        private void Awake()
        {
            _model = new GridModel(rows, cols, blockSprites.Length);
            DisableConflictingLayout();
            BuildCells();
        }

        // The cells are placed by hand (they need to overlap and control their draw order), which a
        // LayoutGroup / ContentSizeFitter would override every frame. Disable one if present so the
        // scene keeps working even if the container was set up with a GridLayoutGroup by mistake.
        private void DisableConflictingLayout()
        {
            if (blocksParent.TryGetComponent(out LayoutGroup layout))
            {
                layout.enabled = false;
                Debug.LogWarning("BoardController: disabled a LayoutGroup on the blocks parent; " +
                                 "cells are positioned by code. Remove it from the container.", this);
            }

            if (blocksParent.TryGetComponent(out ContentSizeFitter fitter))
                fitter.enabled = false;
        }

        private void OnEnable() => gameManager.GameStarted += NewBoard;
        private void OnDisable() => gameManager.GameStarted -= NewBoard;

        // Instantiates the fixed set of cell views once and positions them by hand, centered on
        // blocksParent. Rows overlap by the stud band so a block covers the studs of the one below.
        // Rows are built bottom-first so upper rows end up later in the sibling list and therefore
        // render in front — otherwise the studs would show on every row.
        private void BuildCells()
        {
            _views = new BlockView[rows, cols];

            float vStep = blockSize.y * (1f - topStudRatio); // effective row height once overlapped
            float hStep = blockSize.x + horizontalSpacing;
            float totalWidth = cols * blockSize.x + (cols - 1) * horizontalSpacing;
            float totalHeight = blockSize.y + (rows - 1) * vStep;

            // r goes bottom (rows-1) -> top (0) so the topmost row is the last sibling (drawn in front).
            for (int r = rows - 1; r >= 0; r--)
            {
                for (int c = 0; c < cols; c++)
                {
                    var go = new GameObject($"Block_{r}_{c}",
                        typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(BlockView));

                    var rt = (RectTransform)go.transform;
                    rt.SetParent(blocksParent, false);
                    rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.sizeDelta = blockSize;
                    rt.anchoredPosition = new Vector2(
                        -totalWidth * 0.5f + blockSize.x * 0.5f + c * hStep,
                        totalHeight * 0.5f - blockSize.y * 0.5f - r * vStep);

                    var view = go.GetComponent<BlockView>();
                    view.SetCoords(r, c);
                    view.Clicked += OnBlockClicked;
                    _views[r, c] = view;
                }
            }
        }

        // Fresh random board. Driven by GameManager.GameStarted (initial start and every replay).
        private void NewBoard()
        {
            StopAllCoroutines();
            _busy = false;
            _model.FillRandom();
            RefreshAll();
        }

        private void OnBlockClicked(BlockView view)
        {
            if (_busy || !gameManager.CanPlay) return;

            var collected = _model.Collect(view.Row, view.Col);
            if (collected.Count == 0) return;

            // +N points for N blocks collected; the tap costs one move.
            gameManager.AddScore(collected.Count);
            RefreshAll();              // collected cells now render empty
            gameManager.ConsumeMove(); // may trigger Game Over

            StartCoroutine(RefillAfterDelay());
        }

        private IEnumerator RefillAfterDelay()
        {
            _busy = true;
            yield return new WaitForSeconds(refillDelay);
            _model.CollapseAndRefill();
            RefreshAll();
            _busy = false;
        }

        private void RefreshAll()
        {
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    int color = _model.Get(r, c);
                    if (color == GridModel.Empty) _views[r, c].SetEmpty();
                    else _views[r, c].SetColor(blockSprites[color]);
                }
            }
        }
    }
}
