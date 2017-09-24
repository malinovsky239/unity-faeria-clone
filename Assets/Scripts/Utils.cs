using UnityEngine;

namespace Assets.Scripts
{
    public static class Utils
    {
        public static float Sqr(float x)
        {
            return x * x;
        }

        public static void SetSortingOrderRecursively(GameObject obj, int sortingOrder)
        {
            var renderer = obj.GetComponent<Renderer>();
            renderer.sortingOrder = sortingOrder;

            foreach (Transform child in obj.transform)
            {
                SetSortingOrderRecursively(child.gameObject, sortingOrder);
            }
        }

        public static GameController.Player CellOwnerToPlayer(FieldController.CellOwner cellOwner)
        {
            switch (cellOwner)
            {
                case FieldController.CellOwner.Player1:
                    return GameController.Player.Player1;
                case FieldController.CellOwner.Player2:
                    return GameController.Player.Player2;
                default:
                    return GameController.Player.None;
            }
        }

        public static int Distance(CellController cellA, CellController cellB)
        {
            var dx = cellA.HexX - cellB.HexX;
            var dy = cellA.HexY - cellB.HexY;
            if (dx * dy <= 0)
            {
                return Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy));
            }
            return Mathf.Abs(dx) + Mathf.Abs(dy);
        }
    }
}
