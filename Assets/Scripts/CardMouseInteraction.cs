using UnityEngine;

namespace Assets.Scripts
{
    public class CardMouseInteraction : MonoBehaviour
    {
        public enum MouseInteractionState
        {
            Basic,
            MouseOver,
            Selected,
            NonInteractable
        }

        public MouseInteractionState State { get; private set; }

        private GameController _gameController;
        private FieldController _fieldController;
        private DeckController _deckController;
        private CardController _cardController;
        private GameObject _pointer;

        public void UnlockForMouseInteractions()
        {
            State = MouseInteractionState.Basic;
        }

        private void Awake()
        {
            var gameControllerObject = GameObject.Find(Constants.StringLiterals.GameController);
            _gameController = gameControllerObject.GetComponent<GameController>();
            _fieldController = gameControllerObject.GetComponent<FieldController>();

            var deckObject = GameObject.Find(Constants.StringLiterals.PlayerDeck);
            _deckController = deckObject.GetComponent<DeckController>();

            _cardController = GetComponent<CardController>();

            _pointer = GameObject.Find(Constants.StringLiterals.Pointer);
        }

        private void OnMouseEnter()
        {
            if (_gameController.CurrentMode == GameController.Mode.WaitingForAction &&
                _cardController.IsSelectable() && State == MouseInteractionState.Basic)
            {
                State = MouseInteractionState.MouseOver;
            }
            else if (_gameController.CurrentMode == GameController.Mode.MovingCard
                     && _cardController.CurrentState == CardController.State.OnTheField)
            {
                _cardController.CellController.OnMouseEnter(); // pass MouseOver "through" card's collider
            }
        }

        private void OnMouseExit()
        {
            if (State == MouseInteractionState.MouseOver)
            {
                State = MouseInteractionState.Basic;
            }
        }

        private void OnMouseDown()
        {
            if (_gameController.CurrentMode == GameController.Mode.WaitingForAction
                && _cardController.IsSelectable())
            {
                _pointer.transform.position = transform.position;
                State = MouseInteractionState.Selected;
                switch (_cardController.CurrentState)
                {
                    case CardController.State.InDeck:
                        _gameController.CurrentMode = GameController.Mode.PlacingCard;
                        _fieldController.ShowCellsAvailableForCards(FieldController.CellOwner.Player1);
                        _deckController.SelectCard(gameObject);
                        break;

                    case CardController.State.OnTheField:
                        _gameController.CurrentMode = GameController.Mode.MovingCard;
                        _fieldController.ShowPotentialDestinationCells(_cardController.CellController, GameController.Player.Player1);
                        _fieldController.SelectCard(gameObject);
                        break;
                }
            }
            else if (_gameController.CurrentMode == GameController.Mode.MovingCard
                     && _cardController.CurrentState == CardController.State.OnTheField)
            {
                _cardController.CellController.OnMouseDown(); // pass MouseClick "through" card's collider
            }
        }
    }
}
