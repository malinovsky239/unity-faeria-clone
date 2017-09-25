using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
    public class EndTurn : MonoBehaviour
    {
        [SerializeField] private Sprite _endTurn_border;
        [SerializeField] private Sprite _endTurn_inactive;
        [SerializeField] private Sprite _endTurn_active;
        private SpriteRenderer _spriteRenderer;
        private GameController _gameController;
        private GameObject _terrainButton;
        private GameObject _hourglass;
        private bool _on;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _gameController = GameObject.Find(Constants.StringLiterals.GameController).GetComponent<GameController>();
            _terrainButton = GameObject.Find(Constants.StringLiterals.AddTerrain);
            _hourglass = GameObject.Find(Constants.StringLiterals.Hourglass);
            _terrainButton.SetActive(false);
        }

        private void Start()
        {
            StartCoroutine(Off());
        }

        public void On()
        {
            _terrainButton.SetActive(false);
            _spriteRenderer.sprite = _endTurn_inactive;
            _on = true;
        }

        private IEnumerator Off()
        {
            yield return new WaitUntil(() => !_gameController.IsPlayerMove);
            _on = false;
            _hourglass.SetActive(true);
            _spriteRenderer.sprite = _endTurn_border;
            yield return new WaitUntil(() => _gameController.IsPlayerMove);
            _hourglass.SetActive(false);
            if (!_on)
            {
                _terrainButton.SetActive(true);
            }
        }

        public void GameOver()
        {
            _on = false;
            _hourglass.SetActive(false);
            _terrainButton.SetActive(false);
            _spriteRenderer.sprite = _endTurn_border;
        }

        private void OnMouseEnter()
        {
            if (_on && !_gameController.IsUIBlocked())
            {
                _spriteRenderer.sprite = _endTurn_active;
            }
        }

        private void OnMouseExit()
        {
            if (_on && !_gameController.IsUIBlocked())
            {
                _spriteRenderer.sprite = _endTurn_inactive;
            }
        }

        private void OnMouseDown()
        {
            if (_on && !_gameController.IsUIBlocked())
            {
                StartCoroutine(Off());
                StartCoroutine(_gameController.NextTurn());
            }
        }
    }
}
