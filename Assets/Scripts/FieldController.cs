using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts
{
    public class FieldController : MonoBehaviour
    {
        public enum CellOwner
        {
            Out,
            Empty,
            Player1,
            Player2
        }

        public enum CellContent
        {
            Out,
            Empty,
            Card,
            Orb
        }

        public HexField<CellController> Field;
        public HexField<GameObject> FieldContent;
        public HexField<CellOwner> FieldCellOwner;
        public HexField<CellContent> FieldCellContent;
        public MagicSourceController[] MagicSourceControllers;

        public CellController CellToAttackFrom { get; private set; }
        public bool CellsToAttackInOneMoveExist { get; private set; }

        private List<CellController> _highlightedCellControllers;
        private GameObject _activeCard;

        private FieldHelper _fieldHelper;
        private FieldInitializer _fieldInitializer;

        private void Awake()
        {
            _fieldHelper = GetComponent<FieldHelper>();
            _fieldInitializer = GetComponent<FieldInitializer>();
            MagicSourceControllers = new MagicSourceController[Constants.MagicSourceControllersCount];
        }

        public void TrySetAsCellToAttackFrom(CellController cell)
        {
            if ((FieldCellContent[cell] == CellContent.Empty ||
                 FieldCellContent[cell] == CellContent.Card && FieldContent[cell].GetComponent<CardController>().Owner == GameController.Player.Player1)
                && CellsToAttackInOneMoveExist)
            {
                CellToAttackFrom = cell;
            }
        }

        public void MoveActiveCardOnTheField(CellController destinationCell)
        {
            var cardController = _activeCard.GetComponent<CardController>();
            if (FieldCellContent[destinationCell] != CellContent.Empty)
            {
                var intermediateCell = _fieldHelper.AreNeighbours(destinationCell, CellToAttackFrom)
                    ? CellToAttackFrom
                    : destinationCell.DefaultAttackFrom;
                StartCoroutine(cardController.MoveTo(intermediateCell));
                StartCoroutine(cardController.Attack(FieldContent[destinationCell]));
            }
            else
            {
                StartCoroutine(cardController.MoveTo(destinationCell));
            }
            CellToAttackFrom = null;
        }

        public void ShowPotentialTerrainExpansion(CellOwner player)
        {
            Highlight(GetPotentialTerrainExpansion(player));
        }

        public void ShowCellsAvailableForCards(CellOwner player)
        {
            Highlight(GetCellsAvailableForCards(player));
        }

        public void ShowPotentialDestinationCells(CellController sourceCell, GameController.Player player)
        {
            Highlight(GetPotentialDestinationCells(sourceCell, player));
            sourceCell.MakeActionReady();
            _highlightedCellControllers.Add(sourceCell);
        }

        public List<CellController> GetPotentialTerrainExpansion(CellOwner player)
        {
            var expansionCells = new List<CellController>();
            for (var i = 0; i < FieldCellOwner.VerticalSize; i++)
            {
                for (var j = 0; j < FieldCellOwner.HorizontalSize; j++)
                {
                    if (_fieldHelper.ValidCellCoordinate(i, j) && Field[i, j])
                    {
                        var cell = Field[i, j];
                        if ((HasAdjacentCellOfType(cell, player) ||
                             HasAdjacentCellWithCard(cell, Utils.CellOwnerToPlayer(player))) &&
                            FieldCellOwner[cell] == CellOwner.Empty)
                        {
                            expansionCells.Add(cell);
                        }
                    }
                }
            }
            return expansionCells;
        }

        public List<CellController> GetCellsAvailableForCards(CellOwner player)
        {
            var availableCells = new List<CellController>();
            for (var i = 0; i < FieldCellOwner.VerticalSize; i++)
            {
                for (var j = 0; j < FieldCellOwner.HorizontalSize; j++)
                {
                    if (FieldCellOwner[i, j] == player && FieldCellContent[i, j] == CellContent.Empty)
                    {
                        var cell = Field[i, j];
                        availableCells.Add(cell);
                    }
                }
            }
            return availableCells;
        }

        public List<CellController> GetPotentialDestinationCells(CellController sourceCell, GameController.Player player)
        {
            var tmp = GetAdjacentPassableCells(sourceCell);
            var potentialDestinationCells = tmp.ToList();
            tmp.Insert(0, sourceCell); // adding to the left, so that current position has the highest priority as defaultAttackSource
            var cellsToAttack = GetAdjacentCellsWithOpponentCards(tmp, player);
            CellsToAttackInOneMoveExist = cellsToAttack.Count > 0;
            potentialDestinationCells.AddRange(cellsToAttack);
            return potentialDestinationCells;
        }

        private List<CellController> GetAdjacentPassableCells(CellController cell)
        {
            var adjacentPassableCells = new List<CellController>();
            for (var i = 0; i < Constants.HexCoordShiftsX.Length; i++)
            {
                var adjX = cell.HexX + Constants.HexCoordShiftsX[i];
                var adjY = cell.HexY + Constants.HexCoordShiftsY[i];
                if (_fieldHelper.ValidCellCoordinate(adjX, adjY))
                {
                    var adjCell = Field[adjX, adjY];

                    if ((FieldCellOwner[adjCell] == CellOwner.Player1 ||
                         FieldCellOwner[adjCell] == CellOwner.Player2) &&
                        FieldCellContent[adjCell] == CellContent.Empty)
                    {
                        adjacentPassableCells.Add(adjCell);
                    }
                }
            }
            return adjacentPassableCells;
        }

        public List<CellController> GetAdjacentCellsWithOpponentCards(List<CellController> baseCells,
            GameController.Player player)
        {
            var cellsWithOpponentCards = new HashSet<CellController>();
            foreach (var controller in baseCells)
            {
                var baseHexX = controller.HexX;
                var baseHexY = controller.HexY;
                for (var i = 0; i < Constants.HexCoordShiftsX.Length; i++)
                {
                    var neighbourCoordX = baseHexX + Constants.HexCoordShiftsX[i];
                    var neighbourCoordY = baseHexY + Constants.HexCoordShiftsY[i];
                    if (_fieldHelper.ValidCellCoordinate(neighbourCoordX, neighbourCoordY))
                    {
                        var cellToAttack = Field[neighbourCoordX, neighbourCoordY];
                        if (FieldCellContent[cellToAttack] == CellContent.Card &&
                            FieldContent[cellToAttack].GetComponent<CardController>().Owner != player ||
                            FieldCellContent[cellToAttack] == CellContent.Orb &&
                            Utils.CellOwnerToPlayer(FieldCellOwner[cellToAttack]) != player)
                        {
                            cellToAttack.SetDefaultAttackSource(controller);
                            cellsWithOpponentCards.Add(cellToAttack);
                        }
                    }
                }
            }
            return cellsWithOpponentCards.ToList();
        }

        public void MoveContent(CellController from, CellController to)
        {
            if (from == to)
            {
                return;
            }
            PutCardIntoCell(to, FieldContent[from]);
            ClearCell(from);
        }

        public void ClearCell(CellController cell)
        {
            FieldCellContent[cell] = CellContent.Empty;
            FieldContent[cell] = null;
        }

        public void PutCardIntoCell(CellController cell, GameObject card)
        {
            FieldCellContent[cell] = CellContent.Card;
            FieldContent[cell] = card;
        }

        private void Highlight(List<CellController> controllers)
        {
            _highlightedCellControllers = controllers;
            foreach (var controller in _highlightedCellControllers)
            {
                controller.Highlight();
            }
        }

        public void RemoveCellHighlighting()
        {
            foreach (var cellController in _highlightedCellControllers)
            {
                cellController.Deactivate();
            }
        }

        public void AllowCardsToMove(GameController.Player player)
        {
            for (var i = 0; i < FieldCellContent.VerticalSize; i++)
            {
                for (var j = 0; j < FieldCellContent.HorizontalSize; j++)
                {
                    if (FieldCellContent[i, j] == CellContent.Card &&
                        FieldContent[i, j].GetComponent<CardController>().Owner == player)
                    {
                        var controller = FieldContent[i, j].GetComponent<CardController>();
                        controller.Unfreeze();
                    }
                }
            }
        }

        private bool HasAdjacentCellOfType(CellController cell, CellOwner adjCellType)
        {
            for (var i = 0; i < Constants.HexCoordShiftsX.Length; i++)
            {
                var adjX = cell.HexX + Constants.HexCoordShiftsX[i];
                var adjY = cell.HexY + Constants.HexCoordShiftsY[i];
                if (_fieldHelper.ValidCellCoordinate(adjX, adjY))
                {
                    if (FieldCellOwner[adjX, adjY] == adjCellType)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool HasAdjacentCellWithCard(CellController cell, GameController.Player player)
        {
            for (var i = 0; i < Constants.HexCoordShiftsX.Length; i++)
            {
                var adjX = cell.HexX + Constants.HexCoordShiftsX[i];
                var adjY = cell.HexY + Constants.HexCoordShiftsY[i];
                if (_fieldHelper.ValidCellCoordinate(adjX, adjY))
                {
                    if (FieldCellContent[adjX, adjY] == CellContent.Card &&
                        FieldContent[adjX, adjY].GetComponent<CardController>().Owner == player)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public int CountMagicSourcesUnderControl(GameController.Player player)
        {
            var result = 0;
            foreach (var controller in MagicSourceControllers)
            {
                if (HasAdjacentCellWithCard(controller.Location, player))
                {
                    controller.TakeEnergy(player);
                    result++;
                }
            }
            return result;
        }

        public void RefillMagicSources()
        {
            foreach (var controller in MagicSourceControllers)
            {
                controller.Refill();
            }
        }

        public GameController.Player[] GetMagicSourcesReachabilityInfo()
        {
            var reachingSide = new GameController.Player[Constants.MagicSourceControllersCount];
            for (var i = 0; i < Constants.MagicSourceControllersCount; i++)
            {
                var magicSource = MagicSourceControllers[i];
                var mask = 0;
                if (HasAdjacentCellOfType(magicSource.Location, CellOwner.Player1))
                {
                    mask += 1;
                }
                if (HasAdjacentCellOfType(magicSource.Location, CellOwner.Player2))
                {
                    mask += 2;
                }
                reachingSide[i] = MaskToPlayer(mask);
            }
            return reachingSide;
        }

        public GameController.Player[] GetMagicSourcesControlInfo()
        {
            var controllingSide = new GameController.Player[Constants.MagicSourceControllersCount];
            for (var i = 0; i < Constants.MagicSourceControllersCount; i++)
            {
                var magicSource = MagicSourceControllers[i];
                var mask = 0;
                if (HasAdjacentCellWithCard(magicSource.Location, GameController.Player.Player1))
                {
                    mask += 1;
                }
                if (HasAdjacentCellWithCard(magicSource.Location, GameController.Player.Player2))
                {
                    mask += 2;
                }
                controllingSide[i] = MaskToPlayer(mask);
            }
            return controllingSide;
        }

        private GameController.Player MaskToPlayer(int mask)
        {
            switch (mask)
            {
                case 0:
                    return GameController.Player.None;
                case 1:
                    return GameController.Player.Player1;
                case 2:
                    return GameController.Player.Player2;
                case 3:
                    return GameController.Player.Both;
                default:
                    throw new Exception("Invalid mask");
            }
        }

        public CardLibrary.Card GetCumulativePowerAroundCell(CellController center, int radius,
            GameController.Player player)
        {
            var result = new CardLibrary.Card();
            for (var i = 0; i < Field.VerticalSize; i++)
            {
                for (var j = 0; j < Field.HorizontalSize; j++)
                {
                    if (Field[i, j] && FieldCellContent[i, j] == CellContent.Card)
                    {
                        var otherCell = Field[i, j];
                        if (Utils.Distance(center, otherCell) <= radius)
                        {
                            var card = FieldContent[i, j].GetComponent<CardController>();
                            if (card.Owner == player)
                            {
                                result.Damage += card.GetDamage();
                                result.Life += card.GetHealth();
                            }
                        }
                    }
                }
            }
            return result;
        }

        public CellController GetOrbPosition(GameController.Player player)
        {
            switch (player)
            {
                case GameController.Player.Player1:
                    return Field[_fieldInitializer.CenterX + _fieldInitializer.Radius, _fieldInitializer.CenterY];

                case GameController.Player.Player2:
                    return Field[_fieldInitializer.CenterX - _fieldInitializer.Radius, _fieldInitializer.CenterY];

                default:
                    throw new Exception("Invalid player");
            }
        }

        public void SelectCard(GameObject card)
        {
            _activeCard = card;
        }
    }
}
