using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts
{
    public class NpcDecisionMaker : INpcDecisionMaker
    {
        private enum StrategyMode
        {
            Defensive,
            Explorative,
            Aggressive
        }

        private StrategyMode _currentStrategyMode;
        private NpcBehaviour _npcController;
        private FieldController _fieldController;
        private CellController _npcOrbPosition;
        private CellController _playerOrbPosition;
        private const int ThreatRadius = 2;
        private const int EnergyThreshold = 10;

        public NpcDecisionMaker(NpcBehaviour npcController, FieldController fieldController)
        {
            _npcController = npcController;
            _fieldController = fieldController;
            _npcOrbPosition = _fieldController.GetOrbPosition(GameController.Player.Player2);
            _playerOrbPosition = _fieldController.GetOrbPosition(GameController.Player.Player1);
        }

        public void AdjustStrategyMode()
        {
            var threatToOrb = _fieldController.GetCumulativePowerAroundCell(_npcOrbPosition, ThreatRadius,
                GameController.Player.Player1);
            var orbDefense = _fieldController.GetCumulativePowerAroundCell(_npcOrbPosition, ThreatRadius,
                GameController.Player.Player2);
            if (orbDefense.TurnsToDefeat(threatToOrb) >= threatToOrb.TurnsToDefeat(orbDefense) && threatToOrb.Damage > 0)
            {
                _currentStrategyMode = StrategyMode.Defensive;
                return;
            }
            if (_npcController.CurrentEnergy < EnergyThreshold)
            {
                var magicSourceControlInfo = _fieldController.GetMagicSourcesControlInfo();
                var balance = 0;
                foreach (var controllingSide in magicSourceControlInfo)
                {
                    switch (controllingSide)
                    {
                        case GameController.Player.Player1:
                            balance--;
                            break;

                        case GameController.Player.Player2:
                            balance++;
                            break;
                    }
                }
                if (balance <= 0)
                {
                    _currentStrategyMode = StrategyMode.Explorative;
                    return;
                }
            }
            _currentStrategyMode = StrategyMode.Aggressive;
        }

        public CellController ChooseCellToOccupy(List<CellController> expansionOptions)
        {
            CellController cellToOccupy;
            switch (_currentStrategyMode)
            {
                case StrategyMode.Defensive:
                    cellToOccupy = NpcHelper.ChooseClosestToSpecificLocationCell(_npcOrbPosition, expansionOptions);
                    break;

                case StrategyMode.Explorative:
                    cellToOccupy = NpcHelper.ChooseClosestToUnexploredMagicSourceCell(expansionOptions, _fieldController);
                    break;

                case StrategyMode.Aggressive:
                    cellToOccupy = NpcHelper.ChooseClosestToSpecificLocationCell(_playerOrbPosition, expansionOptions);
                    break;

                default:
                    throw new Exception("Invalid strategy");
            }
            return cellToOccupy;
        }

        public void PlayCardFromDeck(out CardController cardToPlay, out CellController cellToPlace,
            List<CardController> playableCards, List<CellController> availableCells)
        {
            switch (_currentStrategyMode)
            {
                case StrategyMode.Defensive:
                    cellToPlace = NpcHelper.ChooseClosestToSpecificLocationCell(_npcOrbPosition, availableCells);
                    cardToPlay = NpcHelper.ChooseStrongestCard(playableCards);
                    break;

                case StrategyMode.Explorative:
                    cellToPlace = NpcHelper.ChooseClosestToUncontrolledMagicSourceCell(availableCells, _fieldController);
                    cardToPlay = NpcHelper.ChooseWeakestCard(playableCards);
                    break;

                case StrategyMode.Aggressive:
                    cellToPlace = NpcHelper.ChooseClosestToSpecificLocationCell(_playerOrbPosition, availableCells);
                    cardToPlay = NpcHelper.ChooseStrongestCard(playableCards);
                    break;

                default:
                    throw new Exception("Invalid strategy");
            }
        }

        public void PlayCardOnField(ref CellController cellToMove, ref CellController cellToAttack, CardController card,
            List<CellController> destinationCells, List<CellController> cellsToAttack)
        {
            if (card.CellController.HasNeighboringMagicSource && _npcController.CurrentEnergy < EnergyThreshold)
            {
                destinationCells = destinationCells.Where(cell => cell.HasNeighboringMagicSource).ToList();
                var tmp = destinationCells.ToList();
                tmp.Insert(0, card.CellController);
                cellsToAttack = _fieldController.GetAdjacentCellsWithOpponentCards(tmp, GameController.Player.Player2);
            }
            if (cellsToAttack.Count > 0)
            {
                cellToAttack = cellsToAttack[UnityEngine.Random.Range(0, cellsToAttack.Count)];
                cellToMove = cellToAttack.DefaultAttackFrom;
            }
            else if (destinationCells.Count > 0)
            {
                switch (_currentStrategyMode)
                {
                    case StrategyMode.Defensive:
                        cellToMove = NpcHelper.ChooseClosestToSpecificLocationCell(_npcOrbPosition, destinationCells);
                        break;

                    case StrategyMode.Explorative:
                        if (!card.CellController.HasNeighboringMagicSource)
                        {
                            cellToMove = NpcHelper.ChooseClosestToUncontrolledMagicSourceCell(destinationCells,
                                _fieldController);
                        }
                        break;

                    case StrategyMode.Aggressive:
                        cellToMove = NpcHelper.ChooseClosestToSpecificLocationCell(_playerOrbPosition, destinationCells);
                        break;

                    default:
                        throw new Exception("Invalid strategy");
                }
            }
        }
    }
}
