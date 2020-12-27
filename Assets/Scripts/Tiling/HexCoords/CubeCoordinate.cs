using System;
using UnityEngine;

namespace Simulation.Tiling.HexCoords
{
    [Serializable]
    public struct CubeCoordinate
    {
        public int x;
        public int y;
        public int z;

        public CubeCoordinate(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }


        public CubeCoordinate(Vector3 floatCube)
        {
            x = Mathf.RoundToInt(floatCube.x);
            y = Mathf.RoundToInt(floatCube.y);
            z = Mathf.RoundToInt(floatCube.z);

            var xDiff = Mathf.Abs(x - floatCube.x);
            var yDiff = Mathf.Abs(y - floatCube.y);
            var zDiff = Mathf.Abs(z - floatCube.z);

            if (xDiff > yDiff && xDiff > zDiff)
            {
                x = -y - z;
            }
            else if (yDiff > zDiff)
            {
                y = -x - z;
            }
            else
            {
                z = -x - y;
            }
        }

        public OffsetCoordinate ToOffset()
        {
            var col = x;
            var row = z + (x - (x & 1)) / 2;
            return new OffsetCoordinate(col, row);
        }
        public AxialCoordinate ToAxial()
        {
            var q = x;
            var r = z;
            return new AxialCoordinate(q, r);
        }

        public CubeCoordinate GetCoordInLargerHexGrid(int r)
        {
            float area = 3 * r * r + 3 * r + 1;
            int shift = 3 * r + 2;
            var xh = Mathf.FloorToInt((y + (shift * x)) / area);
            var yh = Mathf.FloorToInt((z + (shift * y)) / area);
            var zh = Mathf.FloorToInt((x + (shift * z)) / area);
            var i = Mathf.FloorToInt((1 + xh - yh) / 3f);
            var j = Mathf.FloorToInt((1 + yh - zh) / 3f);
            var k = Mathf.FloorToInt((1 + zh - xh) / 3f);
            return new CubeCoordinate(i, j, k);
        }

        public int DistanceTo(CubeCoordinate other)
        {
            return Mathf.Max(Mathf.Abs(x - other.x), Mathf.Abs(y - other.y), Mathf.Abs(z - other.z));
        }

        public static CubeCoordinate operator +(CubeCoordinate a, CubeCoordinate b)
        {
            return new CubeCoordinate(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static CubeCoordinate operator -(CubeCoordinate a, CubeCoordinate b)
        {
            return new CubeCoordinate(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public static CubeCoordinate operator /(CubeCoordinate a, int b)
        {
            return new CubeCoordinate(a.x / b, a.y / b, a.z / b);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj is CubeCoordinate)
            {
                CubeCoordinate other = (CubeCoordinate)obj;
                return other.x == x && other.y == y && other.z == z;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return $"{x},{y},{z}".GetHashCode();
        }

        public override string ToString()
        {
            return $"Cube Coordinate: ({x}, {y}, {z})";
        }
    }
}
