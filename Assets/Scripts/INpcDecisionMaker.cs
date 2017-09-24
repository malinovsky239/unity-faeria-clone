using System.Collections.Generic;

namespace Assets.Scripts
{
    public interface INpcDecisionMaker
    {
        void AdjustStrategyMode();

        CellController ChooseCellToOccupy(List<CellController> expansionOptions);

        void PlayCardFromDeck(out CardController cardToPlay, out CellController cellToPlace,
            List<CardController> playableCards, List<CellController> availableCells);

        void PlayCardOnField(ref CellController cellToMove, ref CellController cellToAttack, CardController card,
            List<CellController> destinationCells, List<CellController> cellsToAttack);
    }
}