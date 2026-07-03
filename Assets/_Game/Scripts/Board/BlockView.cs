using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PuzzleGame
{
    // Visual + input for a single grid cell. It reports taps to the BoardController and renders
    // whatever colour the model assigns to its (Row, Col). It holds no game logic of its own.
    [RequireComponent(typeof(Image))]
    public class BlockView : MonoBehaviour, IPointerClickHandler
    {
        public int Row { get; private set; }
        public int Col { get; private set; }

        public event Action<BlockView> Clicked;

        private Image _image;

        private void Awake()
        {
            _image = GetComponent<Image>();
            _image.preserveAspect = true; // block sprites are square; cells may not be
        }

        public void SetCoords(int row, int col)
        {
            Row = row;
            Col = col;
        }

        public void SetColor(Sprite sprite)
        {
            _image.sprite = sprite;
            _image.enabled = true; // a disabled Graphic also stops receiving raycasts
        }

        public void SetEmpty() => _image.enabled = false;

        public void OnPointerClick(PointerEventData eventData) => Clicked?.Invoke(this);
    }
}
