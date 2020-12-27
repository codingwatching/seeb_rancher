using System;
using UnityEngine;

namespace Simulation.Tiling.HexCoords
{
    [Serializable]
    public struct OffsetCoordinate
    {
        public int row;
        public int column;

        public OffsetCoordinate(int column, int row)
        {
            this.column = column;
            this.row = row;
        }

        [Obsolete("Should not be used by external classes, this is only public for unit testing purposes")]
        public bool IsInOffsetColumn()
        {
            return Math.Abs(column) % 2 == 0;
        }
        public CubeCoordinate ToCube()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            var offsetShove = IsInOffsetColumn() ? 0 : 1;
#pragma warning restore CS0618 // Type or member is obsolete

            var x = column;
            var z = row - (column - offsetShove) / 2;
            var y = -x - z;

            return new CubeCoordinate(x, y, z);
        }

        public AxialCoordinate ToAxial()
        {
            return ToCube().ToAxial();
        }

        public bool IsZero()
        {
            return row == 0 && column == 0;
        }

        public bool IsBetween(OffsetCoordinate min, OffsetCoordinate max)
        {
            return min.row < row && row < max.row &&
                min.column < column && column < max.column;
        }

        public static explicit operator OffsetCoordinate(Vector2Int d)
        {
            return new OffsetCoordinate(d.x, d.y);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj is OffsetCoordinate)
            {
                OffsetCoordinate other = (OffsetCoordinate)obj;
                return other.column == column && other.row == row;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return $"{column},{row}".GetHashCode();
        }

        public override string ToString()
        {
            return $"Offset column: {column} row: {row}";
        }
    }
}
