using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Tiling.SquareCoords
{
    [Serializable]
    [StructLayout(LayoutKind.Explicit)] // total size: 8bytes
    public struct SquareCoordinate : IBaseCoordinateType, IEquatable<SquareCoordinate>
    {
        public SquareCoordinate(int row, int col)
        {
            this.row = row;
            column = col;
        }
        [FieldOffset(0)] public int row;
        [FieldOffset(4)] public int column;

        public static SquareCoordinate operator +(SquareCoordinate a, SquareCoordinate b)
        {
            return new SquareCoordinate(a.row + b.row, a.column + b.column);
        }
        public static SquareCoordinate operator -(SquareCoordinate a, SquareCoordinate b)
        {
            return new SquareCoordinate(a.row - b.row, a.column - b.column);
        }
        public static SquareCoordinate operator -(SquareCoordinate a)
        {
            return new SquareCoordinate(-a.row, -a.column);
        }

        public static readonly SquareCoordinate UP = new SquareCoordinate(1, 0);
        public static readonly SquareCoordinate DOWN = new SquareCoordinate(-1, 0);
        public static readonly SquareCoordinate RIGHT = new SquareCoordinate(0, 1);
        public static readonly SquareCoordinate LEFT = new SquareCoordinate(0, -1);

        private static readonly Vector2[] squareVerts = new Vector2[] {
                new Vector3(-.5f,-.5f),
                new Vector3(-.5f, .5f),
                new Vector3( .5f, .5f),
                new Vector3( .5f,-.5f),};

        public int ManhattanMagnitude()
        {
            return Math.Abs(column) + Math.Abs(row);
        }

        public static SquareCoordinate FromPositionInPlane(Vector2 positionInPlane)
        {
            var row = Mathf.RoundToInt(positionInPlane.y);
            var col = Mathf.RoundToInt(positionInPlane.x);
            return new SquareCoordinate(row, col);
        }
        public float2 ToPositionInPlane()
        {
            return new float2(column, row);
        }

        /// <summary>
        /// Get a list of vertexes representing the square around the given square coordinate, with a side length of <paramref name="squareScale"/>
        /// </summary>
        /// <param name="coord">The sqaure coordinate</param>
        /// <param name="coordinateSystem">The coordinate system to use to translate the position of the verts</param>
        /// <param name="squareScale">the scale</param>
        /// <returns>an IEnumerable of 4 vertextes representing the square, rotating clockwise around the center</returns>
        public IEnumerable<Vector2> GetSquareVertsAround()
        {
            Vector2 myPos = ToPositionInPlane();
            return squareVerts.Select(x => x + myPos);
        }

        public static int[] GetTileTriangleIDs()
        {
            return new int[]
            {
                0, 1, 2,
                2, 3, 0
            };
        }
        public Bounds GetRawBounds(float sideLength, Matrix4x4 systemTransform)
        {
            var position = systemTransform.MultiplyPoint3x4((Vector2)ToPositionInPlane());
            return new Bounds(position, Vector3.one * sideLength);
        }

        public static float HeuristicDistance(SquareCoordinate origin, SquareCoordinate destination)
        {
            return (origin - destination).ManhattanMagnitude();
        }

        public static SquareCoordinate AtOrigin()
        {
            return new SquareCoordinate(0, 0);
        }

        public override string ToString()
        {
            return $"row: {row}, col: {column}";
        }

        public override int GetHashCode()
        {
            return (row << 16) ^ column;
        }

        public override bool Equals(object obj)
        {
            if (obj is SquareCoordinate coord)
            {
                return Equals(coord);
            }
            return false;
        }

        public bool Equals(SquareCoordinate other)
        {
            return other.row == row && other.column == column;
        }
        public SquareCoordinate NeighborAtIndex(int neighborIndex)
        {
            switch (neighborIndex % 4)
            {
                case 0:
                    return this + UP;
                case 1:
                    return this + DOWN;
                case 2:
                    return this + LEFT;
                case 3:
                    return this + RIGHT;
            }
            return default;
        }
    }
}
