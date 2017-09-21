using UnityEngine;

namespace Assets.Scripts
{
    public class FieldHelper : MonoBehaviour
    {
        private FieldController _fieldController;
        private FieldInitializer _fieldInitializer;

        void Awake()
        {
            _fieldController = GetComponent<FieldController>();
            _fieldInitializer = GetComponent<FieldInitializer>();
        }

        public void SetCellContent(int x, int y, GameObject prefab, float scaleCoeff = 1,
            FieldController.CellOwner cellOwner = FieldController.CellOwner.Out)
        {
            var gameObj = Instantiate(prefab);
            var rendererSize = prefab.GetComponent<SpriteRenderer>().size;
            gameObj.transform.position = _fieldController.Field[x][y].transform.position;
            gameObj.transform.localScale = new Vector3(
                _fieldInitializer.CellOuterRadius * 2 / rendererSize.x * scaleCoeff,
                _fieldInitializer.CellInnerRadius * 2 / rendererSize.y * scaleCoeff);

            _fieldController.FieldContent[x][y] = gameObj;
            _fieldController.FieldCellOwner[x][y] = cellOwner;
        }

        public bool ValidCellCoordinate(int hexX, int hexY)
        {
            return hexX >= 0 && hexX < _fieldController.Field.Length &&
                   hexY >= 0 && hexY < _fieldController.Field[hexX].Length;
        }

        public bool AreNeighbours(CellController cellA, CellController cellB)
        {
            if (!cellA || !cellB)
            {
                return false;
            }
            var dx = cellA.HexX - cellB.HexX;
            var dy = cellA.HexY - cellB.HexY;
            return Mathf.Abs(dx) <= 1 && Mathf.Abs(dy) <= 1 && dx != dy;
        }
    }
}
