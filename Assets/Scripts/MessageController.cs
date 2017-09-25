using UnityEngine;

namespace Assets.Scripts
{
    public class MessageController : MonoBehaviour
    {
        [SerializeField] private GameController _gameController;
        private SpriteRenderer _spriteRenderer;
        private Vector3 _targetScale = Vector3.one * Constants.MessageMaxSizeScaleCoeff;
        private bool _active;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Hide()
        {
            _gameController.OngoingAnimationsCount--;
            transform.localScale = Vector3.zero;
            _active = false;
        }

        public void Show(Sprite message)
        {
            _spriteRenderer.sprite = message;
            _gameController.OngoingAnimationsCount++;
            transform.localScale = Vector3.one;
            _active = true;
        }

        private void Update()
        {
            if (_active)
            {
                if (transform.localScale.magnitude > _targetScale.magnitude)
                {
                    if (_gameController.CurrentMode != GameController.Mode.GameOver)
                    {
                        Hide();
                        _gameController.CurrentMode = GameController.Mode.WaitingForAction;
                    }
                }
                else
                {
                    transform.localScale *= Constants.MessageScaleMultiplier;
                }
            }
        }
    }
}
