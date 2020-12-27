using Assets.Tiling;
using System;

namespace Simulation.Tiling.HexCoords
{
    [Serializable]
    public struct AxialCoordinate : IBaseCoordinateType
    {
        public int q;
        public int r;

        public AxialCoordinate(int q, int r)
        {
            this.q = q;
            this.r = r;
        }

        public CubeCoordinate ToCube()
        {
            var x = q;
            var z = r;
            var y = -x - z;
            return new CubeCoordinate(x, y, z);
        }

        public OffsetCoordinate ToOffset()
        {
            return ToCube().ToOffset();
        }

        public int DistanceTo(AxialCoordinate other)
        {
            return DistanceTo(other.ToCube());
        }

        public int DistanceTo(CubeCoordinate other)
        {
            return ToCube().DistanceTo(other);
        }


        public AxialCoordinate GetNeighbor(int directionIndex)
        {
            return this + GetDirection(directionIndex);
        }

        public static AxialCoordinate GetDirection(int directionIndex)
        {
            switch (directionIndex)
            {
                case 0:
                    return new AxialCoordinate(1, -1);
                case 1:
                    return new AxialCoordinate(0, -1);
                case 2:
                    return new AxialCoordinate(-1, 0);
                case 3:
                    return new AxialCoordinate(-1, 1);
                case 4:
                    return new AxialCoordinate(0, 1);
                case 5:
                    return new AxialCoordinate(1, 0);
            }
            throw new Exception($"neighbor index out of range: {directionIndex}");
        }

        public static AxialCoordinate operator *(AxialCoordinate a, int b)
        {
            return new AxialCoordinate(a.q * b, a.r * b);
        }
        public static AxialCoordinate operator *(int b, AxialCoordinate a)
        {
            return new AxialCoordinate(a.q * b, a.r * b);
        }

        public static AxialCoordinate operator +(AxialCoordinate a, AxialCoordinate b)
        {
            return new AxialCoordinate(a.q + b.q, a.r + b.r);
        }

        public static AxialCoordinate operator -(AxialCoordinate a, AxialCoordinate b)
        {
            return new AxialCoordinate(a.q - b.q, a.r - b.r);
        }

        public static AxialCoordinate operator /(AxialCoordinate a, int b)
        {
            return new AxialCoordinate(a.q / b, a.r / b);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj is AxialCoordinate)
            {
                AxialCoordinate other = (AxialCoordinate)obj;
                return other.q == q && other.r == r;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return q ^ (r << 16);
        }

        public override string ToString()
        {
            return $"Axial Coordinate: q: {q} r: {r})";
        }
    }
}
