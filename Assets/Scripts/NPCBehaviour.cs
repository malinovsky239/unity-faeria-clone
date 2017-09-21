using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts
{
    // purely randomized behaviour for the time being
    public class NPCBehaviour : MonoBehaviour
    {
        public enum NPCTurnStage
        {
            TerrainExpansion,
            PlayCardsFromDeck,
            PlayCardsOnField,
            EndTurn
        }

        public NPCTurnStage TurnStage { get; private set; }
        public int CurrentEnergy { get; set; }

        private GameController _gameController;
        private FieldController _fieldController;
        private List<CardController> _cardsInDeck;
        private List<CardController> _cardsOnField;

        private void Awake()
        {
            _gameController = GetComponent<GameController>();
            _fieldController = GetComponent<FieldController>();
            _cardsInDeck = new List<CardController>();
            _cardsOnField = new List<CardController>();
        }

        public void PutCardIntoDeck(CardController card)
        {
            _cardsInDeck.Add(card);
        }

        public IEnumerator MakeTurn()
        {
            yield return new WaitUntil(AllCardsInDeck);
            StartCoroutine(ExpandTerrain());
            yield return new WaitUntil(() => TurnStage == NPCTurnStage.PlayCardsFromDeck);
            StartCoroutine(PlayCardsFromDeck());
            yield return new WaitUntil(() => TurnStage == NPCTurnStage.PlayCardsOnField);
            StartCoroutine(PlayCardsOnField());
            yield return new WaitUntil(() => TurnStage == NPCTurnStage.EndTurn);
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
                var index = Random.Range(0, expansionOptions.Count);
                var cellToOccupy = expansionOptions[index];
                expansionOptions.Remove(cellToOccupy);
                cellToOccupy.EnhanceTerrain(FieldController.CellOwner.Player2);
                yield return new WaitForSeconds(Constants.Intervals.AfterTerrainExpansionByNPC);
            }
            TurnStage = NPCTurnStage.PlayCardsFromDeck;
        }

        private bool AllCardsInDeck()
        {
            if (_cardsInDeck.Any(card => card.CurrentState != CardController.State.InDeck))
            {
                return false;
            }
            TurnStage = NPCTurnStage.TerrainExpansion;
            return true;
        }

        private IEnumerator PlayCardsFromDeck()
        {
            var playableCards = _cardsInDeck.Where(card => card.IsPlayable()).ToList();
            var availableCells = _fieldController.GetCellsAvailableForCards(FieldController.CellOwner.Player2);
            if (playableCards.Count > 0 && availableCells.Count > 0)
            {
                var cardIndex = Random.Range(0, playableCards.Count);
                var cellIndex = Random.Range(0, availableCells.Count);
                var cardToPlay = playableCards[cardIndex];
                availableCells[cellIndex].PlaceCard(cardToPlay);
                _cardsInDeck.Remove(cardToPlay);
                _cardsOnField.Add(cardToPlay);
                yield return new WaitUntil(() => cardToPlay.CurrentState == CardController.State.OnTheField);
            }
            TurnStage = NPCTurnStage.PlayCardsOnField;
        }

        private IEnumerator PlayCardsOnField()
        {
            foreach (var card in _cardsOnField)
            {
                if (card.CanMoveThisTurn)
                {
                    var destinationCells = _fieldController.GetPotentialDestinationCells(card.CellController, GameController.Player.Player2);
                    var cellsToAttack = destinationCells.FindAll(cell => _fieldController.FieldCellContent[cell.HexX][cell.HexY] != FieldController.CellContent.Empty);
                    if (cellsToAttack.Count > 0)
                    {
                        var cellToAttack = cellsToAttack[Random.Range(0, cellsToAttack.Count)];
                        StartCoroutine(card.MoveTo(cellToAttack.DefaultAttackFrom));
                        yield return new WaitWhile(() => card.CurrentState == CardController.State.MovingOnTheField);
                        StartCoroutine(card.Attack(_fieldController.FieldContent[cellToAttack.HexX][cellToAttack.HexY]));
                        yield return new WaitUntil(() => _gameController.OngoingAnimationsCount == 0);
                    }
                    else
                    {
                        var selectedCell = destinationCells[Random.Range(0, destinationCells.Count)];
                        StartCoroutine(card.MoveTo(selectedCell));
                        yield return new WaitWhile(() => card.CurrentState == CardController.State.MovingOnTheField);
                    }
                }
            }
            TurnStage = NPCTurnStage.EndTurn;
        }
    }
}
