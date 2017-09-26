using System;
using UnityEngine;

namespace Assets.Scripts
{
    public class HexField<T>
    {
        private T[][] _field;
        public int VerticalSize { get; private set; }
        public int HorizontalSize { get; private set; }

        public HexField(int verticalSize, int horizontalSize)
        {
            _field = new T[verticalSize][];
            for (var i = 0; i < verticalSize; i++)
            {
                _field[i] = new T[horizontalSize];
            }
            VerticalSize = verticalSize;
            HorizontalSize = horizontalSize;
        }

        public T this[int hexX, int hexY]
        {
            get { return _field[hexX][hexY]; }
            set { _field[hexX][hexY] = value; }
        }

        public T this[CellController cell]
        {
            get { return _field[cell.HexX][cell.HexY]; }
            set { _field[cell.HexX][cell.HexY] = value; }
        }
    }
}