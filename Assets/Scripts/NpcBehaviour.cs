using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts
{
    public class NpcBehaviour : MonoBehaviour
    {
        public enum NpcTurnStage
        {
            TerrainExpansion,
            PlayCardsFromDeck,
            PlayCardsOnField,
            EndTurn
        }

        public NpcTurnStage TurnStage { get; private set; }
        public int CurrentEnergy { get; set; }

        private GameController _gameController;
        private FieldController _fieldController;
        private List<CardController> _cardsInDeck;
        private List<CardController> _cardsOnField;
        private INpcDecisionMaker _decisionMaker;

        private void Awake()
        {
            _gameController = GetComponent<GameController>();
            _fieldController = GetComponent<FieldController>();
            _cardsInDeck = new List<CardController>();
            _cardsOnField = new List<CardController>();
        }

        private void Start()
        {
            _decisionMaker = new NpcDecisionMaker(this, _fieldController);
        }

        public IEnumerator MakeTurn()
        {
            RemoveKilledCards();
            _decisionMaker.AdjustStrategyMode();
            yield return new WaitUntil(AllCardsInDeck);
            StartCoroutine(ExpandTerrain());
            yield return new WaitUntil(() => TurnStage == NpcTurnStage.PlayCardsFromDeck);
            StartCoroutine(PlayCardsFromDeck());
            yield return new WaitUntil(() => TurnStage == NpcTurnStage.PlayCardsOnField);
            StartCoroutine(PlayCardsOnField());
            yield return new WaitUntil(() => TurnStage == NpcTurnStage.EndTurn);
        }

        private IEnumerator ExpandTerrain()
        {
            for (var i = 0; i < Constants.StartMoveTerrainCellsCount; i++)
            {
                var expansionOptions = _fieldController.GetPotentialTerrainExpansion(FieldController.CellOwner.Player2);
                if (expansionOptions.Count == 0)
                {
                    break;
                }
                CellController cellToOccupy = _decisionMaker.ChooseCellToOccupy(expansionOptions);
                cellToOccupy.EnhanceTerrain(FieldController.CellOwner.Player2);
                yield return new WaitForSeconds(Constants.Intervals.AfterTerrainExpansionByNPC);
            }
            TurnStage = NpcTurnStage.PlayCardsFromDeck;
        }

        private IEnumerator PlayCardsFromDeck()
        {
            var playableCards = _cardsInDeck.Where(card => card.IsPlayable()).ToList();
            var availableCells = _fieldController.GetCellsAvailableForCards(FieldController.CellOwner.Player2);
            while (playableCards.Count > 0 && availableCells.Count > 0)
            {
                CardController cardToPlay;
                CellController cellToPlace;
                _decisionMaker.PlayCardFromDeck(out cardToPlay, out cellToPlace, playableCards, availableCells);
                if (cellToPlace)
                {
                    cellToPlace.PlaceCard(cardToPlay);
                    availableCells.Remove(cellToPlace);
                    _cardsInDeck.Remove(cardToPlay);
                    _cardsOnField.Add(cardToPlay);
                }
                else
                {
                    break;
                }
                yield return new WaitUntil(() => cardToPlay.CurrentState == CardController.State.OnTheField);
                playableCards = _cardsInDeck.Where(card => card.IsPlayable()).ToList();
            }
            TurnStage = NpcTurnStage.PlayCardsOnField;
        }

        private IEnumerator PlayCardsOnField()
        {
            foreach (var currentCard in _cardsOnField)
            {
                if (currentCard.CanMoveThisTurn)
                {
                    var card = currentCard; // to avoid "Access to foreach variable in closure" warning in WaitWhile
                    var destinationCells = _fieldController.GetPotentialDestinationCells(card.CellController, GameController.Player.Player2);
                    var cellsToAttack = destinationCells.FindAll(cell => _fieldController.FieldCellContent[cell] != FieldController.CellContent.Empty);
                    destinationCells = destinationCells.Where(cell => _fieldController.FieldCellContent[cell] == FieldController.CellContent.Empty).ToList();
                    CellController cellToMove = null;
                    CellController cellToAttack = null;
                    _decisionMaker.PlayCardOnField(ref cellToMove, ref cellToAttack, card, destinationCells, cellsToAttack);
                    if (cellToMove)
                    {
                        StartCoroutine(card.MoveTo(cellToMove));
                        yield return new WaitWhile(() => card.CurrentState == CardController.State.MovingOnTheField);
                        if (cellToAttack)
                        {
                            StartCoroutine(
                                card.Attack(_fieldController.FieldContent[cellToAttack]));
                            yield return new WaitUntil(() => _gameController.OngoingAnimationsCount == 0);
                            if (_gameController.CurrentMode == GameController.Mode.GameOver)
                            {
                                break;
                            }
                        }
                    }
                }
            }
            TurnStage = NpcTurnStage.EndTurn;
        }

        private bool AllCardsInDeck()
        {
            if (_cardsInDeck.Any(card => card.CurrentState != CardController.State.InDeck))
            {
                return false;
            }
            TurnStage = NpcTurnStage.TerrainExpansion;
            return true;
        }

        private void RemoveKilledCards()
        {
            _cardsOnField = _cardsOnField.Where(card => card != null).ToList();
        }

        public void PutCardIntoDeck(CardController card)
        {
            _cardsInDeck.Add(card);
        }
    }
}
