using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts
{
    public class MagicSourceController : MonoBehaviour
    {
        private GameController _gameController;
        private SpriteCollection _spriteCollection;
        private SpriteRenderer _spriteRenderer;
        [SerializeField] private GameObject _bubblePrefab;
        private List<GameObject> _bubbles;

        public CellController Location { get; private set; }

        private void Awake()
        {
            var gameControllerObject = GameObject.Find(Constants.StringLiterals.GameController);
            _gameController = gameControllerObject.GetComponent<GameController>();
            _spriteCollection = gameControllerObject.GetComponent<SpriteCollection>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _bubbles = new List<GameObject>();
        }

        public void SetLocation(CellController location)
        {
            Location = location;
        }

        public void Refill()
        {
            _spriteRenderer.sprite = _spriteCollection.FilledMagicSource;
        }

        public void TakeEnergy(GameController.Player player)
        {
            _spriteRenderer.sprite = _spriteCollection.EmptyMagicSource;
            RemoveDeadBubbles();
            foreach (var bubble in _bubbles)
            {
                bubble.GetComponent<BubbleController>().GoToOrb(_gameController.GetOrbPositionByOwner(player));
            }
        }

        public bool TryTakeEnergy(GameController.Player player)
        {
            if (HasEnergy())
            {
                TakeEnergy(player);
                return true;
            }
            return false;
        }

        private void Update()
        {
            RemoveDeadBubbles();
            if (HasEnergy() && _bubbles.Count < Constants.Bubble.MaxNumberPerMagicSource)
            {
                var newBubble = Instantiate(_bubblePrefab);
                newBubble.transform.SetParent(transform, false);
                _bubbles.Add(newBubble);
            }
        }

        private void RemoveDeadBubbles()
        {
            _bubbles = _bubbles.Where(elem => elem != null).ToList();
        }

        private bool HasEnergy()
        {
            return _spriteRenderer.sprite == _spriteCollection.FilledMagicSource;
        }
    }
}
