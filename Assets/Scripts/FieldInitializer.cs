using System;
using UnityEngine;

namespace Assets.Scripts
{
    public class FieldInitializer : MonoBehaviour
    {
        [SerializeField] private GameObject _cellPrefab;
        [SerializeField] private GameObject _magicSourcePrefab;
        [SerializeField] private GameObject _orbPrefab;

        [SerializeField] private Sprite _smileySuperman;
        [SerializeField] private Sprite _smileyDevil;
        [SerializeField] private Sprite _emptyCell;

        private const int FieldMaxVertDiameter = 7; // Should be odd
        private const int FieldMinVertDiameter = 4;

        private static float _diagonalShiftX;
        private static float _diagonalShiftY;
        private static float _verticalShiftX;
        private static float _verticalShiftY;

        [SerializeField] private float _cellSide;
        public float CellInnerRadius { get; private set; }
        public float CellOuterRadius { get; private set; }

        public int CenterX { get; private set; }
        public int CenterY { get; private set; }
        public int Radius { get; private set; }
        private int[] _cornersX;
        private int[] _cornersY;

        private FieldController _fieldController;
        private FieldHelper _fieldHelper;
        private GameObject _board;

        private void Awake()
        {
            _fieldController = GetComponent<FieldController>();
            _fieldHelper = GetComponent<FieldHelper>();
            _board = GameObject.Find(Constants.StringLiterals.Board);
        }

        public void Initialize()
        {
            CalculateDimesions();
            CreateField();
            InitField();
            SetOrbs();
            SetMagicSources();
        }

        private void CalculateDimesions()
        {
            CellOuterRadius = _cellSide / 2;
            CellInnerRadius = CellOuterRadius * Constants.Sin60;

            _diagonalShiftY = (CellInnerRadius * 2 + Constants.CellPadding) * Constants.Sin30;
            _diagonalShiftX = (CellInnerRadius * 2 + Constants.CellPadding) * Constants.Sin60;
            _verticalShiftX = 0;
            _verticalShiftY = CellInnerRadius * 2 + Constants.CellPadding;

            CenterX = FieldMaxVertDiameter / 2;
            CenterY = FieldMaxVertDiameter - FieldMinVertDiameter;
        }

        private void CreateField()
        {
            _fieldController.Field = new GameObject[FieldMaxVertDiameter][];
            _fieldController.FieldContent = new GameObject[FieldMaxVertDiameter][];
            _fieldController.FieldCellOwner = new FieldController.CellOwner[FieldMaxVertDiameter][];
            _fieldController.FieldCellContent = new FieldController.CellContent[FieldMaxVertDiameter][];
            var horizontalSize = (FieldMaxVertDiameter - FieldMinVertDiameter) * 2 + 1;
            for (var i = 0; i < FieldMaxVertDiameter; i++)
            {
                _fieldController.Field[i] = new GameObject[horizontalSize];
                _fieldController.FieldCellOwner[i] = new FieldController.CellOwner[horizontalSize];
                _fieldController.FieldCellContent[i] = new FieldController.CellContent[horizontalSize];
                _fieldController.FieldContent[i] = new GameObject[horizontalSize];
            }
        }

        private void InitField()
        {
            _fieldController.Field[CenterX][CenterY] = CreateCell(new Vector2(0, 0), CenterX, CenterY);
            for (var i = 0; i < FieldMaxVertDiameter / 2; i++)
            {
                AddNeighbour(CenterX - i, CenterY, Constants.Neighbours.Up);
                AddNeighbour(CenterX + i, CenterY, Constants.Neighbours.Down);
            }

            for (var i = 0; i < FieldMaxVertDiameter - FieldMinVertDiameter; i++)
            {
                for (var j = 0; j < FieldMaxVertDiameter - i - 1; j++)
                {
                    AddNeighbour(j + i, CenterY - i, Constants.Neighbours.DownLeft);
                    AddNeighbour(j, CenterY + i, Constants.Neighbours.DownRight);
                }
            }
        }

        private void SetMagicSources()
        {
            _cornersX = new int[Constants.MagicSourceControllersCount];
            _cornersY = new int[Constants.MagicSourceControllersCount];
            var radius = FieldMaxVertDiameter / 2;
            for (var i = 2; i < 6; i++) // UpLeft, DownRight, DownLeft & UpRight
            {
                _cornersX[i - 2] = CenterX + Constants.HexCoordShiftsX[i] * radius;
                _cornersY[i - 2] = CenterY + Constants.HexCoordShiftsY[i] * radius;
            }

            for (var i = 0; i < Constants.MagicSourceControllersCount; i++)
            {
                _fieldHelper.SetCellContent(_cornersX[i], _cornersY[i], _magicSourcePrefab, Constants.MagicSourceScaleCoeff);
                var magicSource = _fieldController.FieldContent[_cornersX[i]][_cornersY[i]].GetComponent<MagicSourceController>();
                magicSource.SetLocation(_fieldController.Field[_cornersX[i]][_cornersY[i]].GetComponent<CellController>());
                _fieldController.MagicSourceControllers[i] = magicSource;
                for (var j = 0; j < Constants.HexCoordShiftsX.Length; j++)
                {
                    var newX = _cornersX[i] + Constants.HexCoordShiftsX[j];
                    var newY = _cornersY[i] + Constants.HexCoordShiftsY[j];
                    if (_fieldHelper.ValidCellCoordinate(newX, newY) && _fieldController.Field[newX][newY])
                    {
                        _fieldController.Field[newX][newY].GetComponent<CellController>().SetNeighboringMagicSource(magicSource);
                    }
                }
            }
        }

        private void SetOrbs()
        {
            Radius = FieldMaxVertDiameter / 2;
            for (var i = -1; i <= 1; i += 2)
            {
                var coordX = CenterX + Constants.HexCoordShiftsX[(i + 1) / 2] * Radius;
                var coordY = CenterY + Constants.HexCoordShiftsY[(i + 1) / 2] * Radius;
                _fieldController.Field[coordX][coordY].GetComponent<SpriteRenderer>().sprite = _emptyCell;
                _fieldController.FieldCellContent[coordX][coordY] = FieldController.CellContent.Orb;
                _fieldHelper.SetCellContent(coordX, coordY, _orbPrefab);
            }
            _fieldController.FieldCellOwner[CenterX + Radius][CenterY] = FieldController.CellOwner.Player1;
            _fieldController.FieldCellOwner[CenterX - Radius][CenterY] = FieldController.CellOwner.Player2;

            var enemy = _fieldController.FieldContent[CenterX - Radius][CenterY].GetComponent<OrbController>();
            enemy.SetOwner(GameController.Player.Player2);
            enemy.SetHero(_smileyDevil, Constants.SmileyDevilScaleCoeff);
        }

        private void AddNeighbour(int hexX, int hexY, Constants.Neighbours type)
        {
            float shiftX, shiftY;
            switch (type)
            {
                case Constants.Neighbours.DownLeft:
                    shiftX = -_diagonalShiftX;
                    shiftY = -_diagonalShiftY;
                    break;

                case Constants.Neighbours.DownRight:
                    shiftX = _diagonalShiftX;
                    shiftY = -_diagonalShiftY;
                    break;

                case Constants.Neighbours.Up:
                    shiftX = _verticalShiftX;
                    shiftY = _verticalShiftY;
                    break;

                case Constants.Neighbours.Down:
                    shiftX = _verticalShiftX;
                    shiftY = -_verticalShiftY;
                    break;

                default:
                    throw new Exception("Wrong shift direction");
            }
            var shiftHexX = Constants.HexCoordShiftsX[(int)type];
            var shiftHexY = Constants.HexCoordShiftsY[(int)type];
            var cell = _fieldController.Field[hexX][hexY];
            _fieldController.Field[hexX + shiftHexX][hexY + shiftHexY] =
                CreateCell(cell.transform.position + new Vector3(shiftX, shiftY),
                           hexX + shiftHexX, hexY + shiftHexY);
        }

        private GameObject CreateCell(Vector2 shift, int hexX, int hexY)
        {
            var cell = Instantiate(_cellPrefab);

            var cellController = cell.GetComponent<CellController>();
            cellController.SetCoordinates(hexX, hexY);

            var rendererSize = cell.GetComponent<SpriteRenderer>().size;
            cell.transform.localScale =
                new Vector3(CellOuterRadius * 2 / rendererSize.x,
                            CellInnerRadius * 2 / rendererSize.y);
            cell.transform.position = new Vector3(shift.x, shift.y);
            cell.transform.parent = _board.transform;

            _fieldController.FieldCellOwner[hexX][hexY] = FieldController.CellOwner.Empty;
            _fieldController.FieldCellContent[hexX][hexY] = FieldController.CellContent.Empty;

            return cell;
        }
    }
}
