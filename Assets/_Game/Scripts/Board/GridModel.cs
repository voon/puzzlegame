using System.Collections.Generic;
using UnityEngine;

namespace PuzzleGame
{
    // Pure game-logic model for the block grid. No MonoBehaviour and no scene references,
    // so the collection and gravity rules are easy to reason about and to unit-test.
    // Each cell holds a colour index in [0, colorCount); Empty (-1) marks a hole to refill.
    public class GridModel
    {
        public const int Empty = -1;

        public int Rows { get; }
        public int Cols { get; }

        private readonly int _colorCount;
        private readonly int[,] _cells;

        public GridModel(int rows, int cols, int colorCount)
        {
            Rows = rows;
            Cols = cols;
            _colorCount = colorCount;
            _cells = new int[rows, cols];
        }

        public int Get(int row, int col) => _cells[row, col];

        // Fills the whole grid with random colours (used on start and on replay).
        public void FillRandom()
        {
            for (int r = 0; r < Rows; r++)
                for (int c = 0; c < Cols; c++)
                    _cells[r, c] = RandomColor();
        }

        // Flood-fills from (row, col) collecting every orthogonally-connected cell of the same
        // colour, clears them (sets Empty) and returns their coordinates. Returns an empty list
        // if the tapped cell was already empty.
        public List<Vector2Int> Collect(int row, int col)
        {
            var collected = new List<Vector2Int>();
            int target = _cells[row, col];
            if (target == Empty) return collected;

            FloodFill(row, col, target, collected);
            foreach (var cell in collected) _cells[cell.x, cell.y] = Empty;
            return collected;
        }

        // Recursive flood fill. Cells are only cleared after traversal, so the visited guard
        // (not the cell value) is what stops the recursion.
        private void FloodFill(int row, int col, int target, List<Vector2Int> visited)
        {
            if (row < 0 || row >= Rows || col < 0 || col >= Cols) return;
            if (_cells[row, col] != target) return;
            if (visited.Contains(new Vector2Int(row, col))) return;

            visited.Add(new Vector2Int(row, col));

            FloodFill(row - 1, col, target, visited);
            FloodFill(row + 1, col, target, visited);
            FloodFill(row, col - 1, target, visited);
            FloodFill(row, col + 1, target, visited);
        }

        // Instant gravity + refill: within each column the surviving blocks fall to the bottom
        // keeping their relative order, and the holes left on top are filled with new random blocks.
        public void CollapseAndRefill()
        {
            for (int c = 0; c < Cols; c++)
            {
                // Surviving colours, read top-to-bottom.
                var survivors = new List<int>(Rows);
                for (int r = 0; r < Rows; r++)
                    if (_cells[r, c] != Empty) survivors.Add(_cells[r, c]);

                int holes = Rows - survivors.Count;
                for (int r = 0; r < Rows; r++)
                    _cells[r, c] = r < holes ? RandomColor() : survivors[r - holes];
            }
        }

        private int RandomColor() => Random.Range(0, _colorCount);
    }
}
