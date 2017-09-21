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

        public GameObject[][] Field;
        public GameObject[][] FieldContent;
        public CellOwner[][] FieldCellOwner;
        public CellContent[][] FieldCellContent;
        public MagicSourceController[] MagicSourceControllers;

        public CellController CellToAttackFrom { get; private set; }
        public bool CellsToAttackInOneMoveExist { get; private set; }

        private List<CellController> _highlightedCellControllers;
        private GameObject _activeCard;

        private FieldHelper _fieldHelper;

        private void Awake()
        {
            _fieldHelper = GetComponent<FieldHelper>();
            MagicSourceControllers = new MagicSourceController[Constants.MagicSourceControllersCount];
        }

        public void TrySetAsCellToAttackFrom(CellController cell)
        {
            if (CellsToAttackInOneMoveExist && FieldCellContent[cell.HexX][cell.HexY] == CellContent.Empty)
            {
                CellToAttackFrom = cell;
            }
        }

        public void MoveActiveCardOnTheField(CellController destinationCell)
        {
            var cardController = _activeCard.GetComponent<CardController>();
            if (FieldCellContent[destinationCell.HexX][destinationCell.HexY] != CellContent.Empty)
            {
                var intermediateCell = _fieldHelper.AreNeighbours(destinationCell, CellToAttackFrom)
                    ? CellToAttackFrom
                    : destinationCell.DefaultAttackFrom;
                StartCoroutine(cardController.MoveTo(intermediateCell));
                StartCoroutine(cardController.Attack(FieldContent[destinationCell.HexX][destinationCell.HexY]));
            }
            else
            {
                StartCoroutine(cardController.MoveTo(destinationCell));
            }
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
        }

        public List<CellController> GetPotentialTerrainExpansion(CellOwner player)
        {
            var expansionCells = new List<CellController>();
            for (var i = 0; i < FieldCellOwner.Length; i++)
            {
                for (var j = 0; j < FieldCellOwner[i].Length; j++)
                {
                    if (_fieldHelper.ValidCellCoordinate(i, j) && Field[i][j])
                    {
                        var cell = Field[i][j].GetComponent<CellController>();
                        if ((HasAdjacentCellOfType(cell, player) ||
                             HasAdjacentCellWithCard(cell, Utils.CellOwnerToPlayer(player))) &&
                            FieldCellOwner[i][j] == CellOwner.Empty)
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
            for (var i = 0; i < FieldCellOwner.Length; i++)
            {
                for (var j = 0; j < FieldCellOwner[i].Length; j++)
                {
                    if (FieldCellOwner[i][j] == player && FieldCellContent[i][j] == CellContent.Empty)
                    {
                        var controller = Field[i][j].GetComponent<CellController>();
                        availableCells.Add(controller);
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
                    if ((FieldCellOwner[adjX][adjY] == CellOwner.Player1 ||
                         FieldCellOwner[adjX][adjY] == CellOwner.Player2) &&
                        FieldCellContent[adjX][adjY] == CellContent.Empty)
                    {
                        var controller = Field[adjX][adjY].GetComponent<CellController>();
                        adjacentPassableCells.Add(controller);
                    }
                }
            }
            return adjacentPassableCells;
        }

        private List<CellController> GetAdjacentCellsWithOpponentCards(List<CellController> baseCells,
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
                        if (FieldCellContent[neighbourCoordX][neighbourCoordY] == CellContent.Card &&
                            FieldContent[neighbourCoordX][neighbourCoordY].GetComponent<CardController>().Owner != player ||
                            FieldCellContent[neighbourCoordX][neighbourCoordY] == CellContent.Orb &&
                            Utils.CellOwnerToPlayer(FieldCellOwner[neighbourCoordX][neighbourCoordY]) != player)
                        {
                            var cellToAttack = Field[neighbourCoordX][neighbourCoordY].GetComponent<CellController>();
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
            PutCardIntoCell(to, FieldContent[from.HexX][from.HexY]);
            ClearCell(from);
        }

        public void ClearCell(CellController cell)
        {
            FieldCellContent[cell.HexX][cell.HexY] = CellContent.Empty;
            FieldContent[cell.HexX][cell.HexY] = null;
        }

        public void PutCardIntoCell(CellController cell, GameObject card)
        {
            FieldCellContent[cell.HexX][cell.HexY] = CellContent.Card;
            FieldContent[cell.HexX][cell.HexY] = card;
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
            for (var i = 0; i < FieldCellContent.Length; i++)
            {
                for (var j = 0; j < FieldCellContent[i].Length; j++)
                {
                    if (FieldCellContent[i][j] == CellContent.Card &&
                        FieldContent[i][j].GetComponent<CardController>().Owner == player)
                    {
                        var controller = FieldContent[i][j].GetComponent<CardController>();
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
                    if (FieldCellOwner[adjX][adjY] == adjCellType)
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
                    if (FieldCellContent[adjX][adjY] == CellContent.Card &&
                        FieldContent[adjX][adjY].GetComponent<CardController>().Owner == player)
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

        public void SelectCard(GameObject card)
        {
            _activeCard = card;
        }
    }
}
