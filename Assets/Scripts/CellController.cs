using UnityEngine;

namespace Assets.Scripts
{
    public class CellController : MonoBehaviour
    {
        [SerializeField] private Sprite _inactive;
        [SerializeField] private Sprite _active;
        [SerializeField] private Sprite _terrain1;
        [SerializeField] private Sprite _terrain2;
        [SerializeField] private Sprite _terrain1_selected;
        [SerializeField] private Sprite _terrain2_selected;
        [SerializeField] private Sprite _terrain1_attacked;
        [SerializeField] private Sprite _terrain2_attacked;
        [SerializeField] private Sprite _orbLocation;
        [SerializeField] private Sprite _orbLocation_attacked;

        private GameObject _gameControllerObject;
        private GameController _gameController;
        private FieldController _fieldController;
        private DeckController _playerDeckController;
        private SpriteRenderer _spriteRenderer;

        public int HexX { get; private set; }
        public int HexY { get; private set; }
        public CellController DefaultAttackFrom { get; private set; }
        public bool HasNeighboringMagicSource { get; private set; }
        private MagicSourceController _neighboringMagicSource;
        private bool _actionReady;

        private void Awake()
        {
            _gameControllerObject = GameObject.Find(Constants.StringLiterals.GameController);
            _gameController = _gameControllerObject.GetComponent<GameController>();
            _fieldController = _gameControllerObject.GetComponent<FieldController>();

            _playerDeckController = GameObject.Find(Constants.StringLiterals.PlayerDeck).GetComponent<DeckController>();

            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void EnhanceTerrain(FieldController.CellOwner player)
        {
            if (player == FieldController.CellOwner.Player1)
            {
                _spriteRenderer.sprite = _terrain1;
                _fieldController.FieldCellOwner[HexX][HexY] = FieldController.CellOwner.Player1;
                _gameController.DecrementTerrainCellsCount();
            }
            else
            {
                _spriteRenderer.sprite = _terrain2;
                _fieldController.FieldCellOwner[HexX][HexY] = FieldController.CellOwner.Player2;
            }
        }

        public void PlaceCard(CardController cardController)
        {
            cardController.GoToField(this);
            _fieldController.PutCardIntoCell(this, cardController.gameObject);
        }

        public void PostProcessCard(GameController.Player cardOwner)
        {
            if (_neighboringMagicSource && _neighboringMagicSource.TryTakeEnergy(cardOwner))
            {
                _gameController.ChangeEnergy(Constants.ControlledMagicSourceEnergyBonus, cardOwner);
            }
        }

        private void OnMouseOver()
        {
            if (_gameController.CurrentMode == GameController.Mode.MovingCard && _actionReady)
            {
                _fieldController.TrySetAsCellToAttackFrom(this);
            }
        }

        public void OnMouseDown()
        {
            if (_actionReady)
            {
                switch (_gameController.CurrentMode)
                {
                    case GameController.Mode.EnhancingTerrain1:
                        EnhanceTerrain(FieldController.CellOwner.Player1);
                        break;

                    case GameController.Mode.PlacingCard:
                        var cardController = _playerDeckController.ActiveCard.GetComponent<CardController>();
                        PlaceCard(cardController);
                        break;

                    case GameController.Mode.MovingCard:
                        _fieldController.MoveActiveCardOnTheField(this);
                        break;
                }
                _gameController.SwitchToWaitingMode();
            }
        }

        public void Highlight()
        {
            if (_spriteRenderer.sprite == _inactive)
            {
                _spriteRenderer.sprite = _active;
            }
            if (_spriteRenderer.sprite == _terrain1)
            {
                _spriteRenderer.sprite = _fieldController.FieldCellContent[HexX][HexY] == FieldController.CellContent.Empty
                                         ? _terrain1_selected : _terrain1_attacked;
            }
            if (_spriteRenderer.sprite == _terrain2)
            {
                _spriteRenderer.sprite = _fieldController.FieldCellContent[HexX][HexY] == FieldController.CellContent.Empty
                                         ? _terrain2_selected : _terrain2_attacked;
            }
            if (_spriteRenderer.sprite == _orbLocation)
            {
                _spriteRenderer.sprite = _orbLocation_attacked;
            }
            _actionReady = true;
        }

        public void Deactivate()
        {
            _actionReady = false;
            if (_spriteRenderer.sprite == _active)
            {
                _spriteRenderer.sprite = _inactive;
            }
            if (_spriteRenderer.sprite == _terrain1_selected || _spriteRenderer.sprite == _terrain1_attacked)
            {
                _spriteRenderer.sprite = _terrain1;
            }
            if (_spriteRenderer.sprite == _terrain2_selected || _spriteRenderer.sprite == _terrain2_attacked)
            {
                _spriteRenderer.sprite = _terrain2;
            }
            if (_spriteRenderer.sprite == _orbLocation_attacked)
            {
                _spriteRenderer.sprite = _orbLocation;
            }
        }

        public void SetNeighboringMagicSource(MagicSourceController magicSource)
        {
            _neighboringMagicSource = magicSource;
            HasNeighboringMagicSource = true;
        }

        public void SetCoordinates(int x, int y)
        {
            HexX = x;
            HexY = y;
        }

        public void SetDefaultAttackSource(CellController controller)
        {
            DefaultAttackFrom = controller;
        }
    }
}
