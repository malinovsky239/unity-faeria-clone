using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public static class NpcHelper
    {
        public static CellController ChooseClosestToSpecificLocationCell(CellController location,
            List<CellController> options)
        {
            var bestCells = new List<CellController>();
            var bestDistance = Int32.MaxValue;
            foreach (var cellCandidate in options)
            {
                var distance = Utils.Distance(cellCandidate, location);
                if (distance == bestDistance)
                {
                    bestCells.Add(cellCandidate);
                }
                if (distance < bestDistance)
                {
                    bestCells.Clear();
                    bestCells.Add(cellCandidate);
                    bestDistance = distance;
                }
            }
            var index = UnityEngine.Random.Range(0, bestCells.Count);
            return bestCells[index];
        }

        public static CardController ChooseStrongestCard(List<CardController> candidates)
        {
            return ChooseBestCard(candidates, (x, y) => x.GetDamage() > y.GetDamage());
        }

        public static CardController ChooseWeakestCard(List<CardController> candidates)
        {
            return ChooseBestCard(candidates, (x, y) => x.GetDamage() < y.GetDamage());
        }

        private static CardController ChooseBestCard(List<CardController> candidates,
            Func<CardController, CardController, bool> isLeftBetter)
        {
            var result = new List<CardController>();
            foreach (var card in candidates)
            {
                if (result.Count > 0 && card.GetDamage() == result[0].GetDamage())
                {
                    result.Add(card);
                }
                if (result.Count == 0 || isLeftBetter(card, result[0]))
                {
                    result.Clear();
                    result.Add(card);
                }
            }
            var index = UnityEngine.Random.Range(0, result.Count);
            return result[index];
        }

        public static CellController ChooseClosestToUnexploredMagicSourceCell(List<CellController> options,
            FieldController fieldController)
        {
            return ChooseCellBasedOnMagicSources(options, fieldController.GetMagicSourcesReachabilityInfo(),
                fieldController);
        }

        public static CellController ChooseClosestToUncontrolledMagicSourceCell(List<CellController> options,
            FieldController fieldController)
        {
            return ChooseCellBasedOnMagicSources(options, fieldController.GetMagicSourcesControlInfo(), fieldController);
        }

        private static CellController ChooseCellBasedOnMagicSources(List<CellController> options,
            GameController.Player[] magicSourcesInfo, FieldController fieldController)
        {
            CellController bestCell = null;
            var bestDistToUnexploredByBoth = Int32.MaxValue;
            var bestDistToUnexploredByNPC = Int32.MaxValue;
            foreach (var cellCandidate in options)
            {
                var minDistToUnexploredByBoth = Int32.MaxValue;
                var minDistToUnexploredByNPC = Int32.MaxValue;
                for (var i = 0; i < Constants.MagicSourceControllersCount; i++)
                {
                    var controllingSide = magicSourcesInfo[i];
                    var distance = Utils.Distance(cellCandidate, fieldController.MagicSourceControllers[i].Location);
                    switch (controllingSide)
                    {
                        case GameController.Player.None:
                            minDistToUnexploredByBoth = Mathf.Min(minDistToUnexploredByBoth, distance);
                            break;

                        case GameController.Player.Player1:
                            minDistToUnexploredByNPC = Mathf.Min(minDistToUnexploredByNPC, distance);
                            break;
                    }
                }
                if (minDistToUnexploredByBoth < bestDistToUnexploredByBoth)
                {
                    bestCell = cellCandidate;
                    bestDistToUnexploredByBoth = minDistToUnexploredByBoth;
                    bestDistToUnexploredByNPC = minDistToUnexploredByNPC;
                }
                if (minDistToUnexploredByBoth == bestDistToUnexploredByBoth &&
                    minDistToUnexploredByNPC < bestDistToUnexploredByNPC)
                {
                    bestCell = cellCandidate;
                    bestDistToUnexploredByNPC = minDistToUnexploredByNPC;
                }
            }
            if (!bestCell)
            {
                var index = UnityEngine.Random.Range(0, options.Count);
                bestCell = options[index];
            }
            return bestCell;
        }
    }
}
