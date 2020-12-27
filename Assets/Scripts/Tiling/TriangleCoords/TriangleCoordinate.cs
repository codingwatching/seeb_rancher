using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Tiling.TriangleCoords
{
    [Serializable]
    [StructLayout(LayoutKind.Explicit)] // total size: 12bytes
    public struct TriangleCoordinate : IBaseCoordinateType, IEquatable<TriangleCoordinate>
    {
        [FieldOffset(0)] public int u;
        [FieldOffset(4)] public int v;
        [FieldOffset(8)] public bool R;

        public static readonly float2 uBasis = new float2(1, 0);
        public static readonly float2 vBasis = new float2(0.5f, Mathf.Sqrt(3) / 2f);

        /// <summary>
        /// inverse matrix of u and v basis, first coord being u. Used to transform from x - y space to u - v space
        /// </summary>
        private static readonly float2 xBasis = new float2(1, 0);
        private static readonly float2 yBasis = new float2(-1f / Mathf.Sqrt(3), 2f / Mathf.Sqrt(3));

        public static readonly float2 rBasis = new float2(0.5f, 1 / (Mathf.Sqrt(3) * 2f)) / 2;

        public TriangleCoordinate(int u, int v, bool R)
        {
            this.u = u;
            this.v = v;
            this.R = R;
        }

        public static TriangleCoordinate FromPositionInPlane(Vector2 positionInPlane)
        {
            var xComponent = positionInPlane.x * xBasis;
            var yComponent = positionInPlane.y * yBasis;
            var coordCenter = xComponent + yComponent;
            var roundedPoint = new TriangleCoordinate(
                Mathf.RoundToInt(coordCenter.x),
                Mathf.RoundToInt(coordCenter.y),
                false);

            var relativePosInSquare = new Vector2(coordCenter.x - roundedPoint.u, coordCenter.y - roundedPoint.v);

            roundedPoint.R = relativePosInSquare.x + relativePosInSquare.y > 0;

            return roundedPoint;
        }
        public float2 ToPositionInPlane()
        {
            var uComponent = u * uBasis;
            var vComponent = v * vBasis;
            var realCoord = uComponent + vComponent;
            realCoord += (R ? 1 : -1) * rBasis;
            return realCoord;
        }

        public TriangleCoordinate NeighborAtIndex(int neighborIndex)
        {
            if (R)
            {
                switch (neighborIndex % 3)
                {
                    case 0:
                        return new TriangleCoordinate
                        {
                            u = u + 1,
                            v = v,
                            R = false
                        };
                    case 1:
                        return new TriangleCoordinate
                        {
                            u = u,
                            v = v,
                            R = false
                        };
                    case 2:
                        return new TriangleCoordinate
                        {
                            u = u,
                            v = v + 1,
                            R = false
                        };
                }
            }
            else
            {
                switch (neighborIndex % 3)
                {
                    case 0:
                        return new TriangleCoordinate
                        {
                            u = u,
                            v = v,
                            R = true
                        };
                    case 1:
                        return new TriangleCoordinate
                        {
                            u = u,
                            v = v - 1,
                            R = true
                        };
                    case 2:
                        return new TriangleCoordinate
                        {
                            u = u - 1,
                            v = v,
                            R = true
                        };
                }
            }
            return default;
        }

        public static float HeuristicDistance(TriangleCoordinate origin, TriangleCoordinate destination)
        {
            return Vector2.SqrMagnitude(origin.ToPositionInPlane() - destination.ToPositionInPlane());
        }


        /// <summary>
        /// Get a list of vertexes representing the triangle around this coordinate,
        ///     with a side length of <paramref name="sideLength"/>
        /// </summary>
        /// <param name="sideLength">the side length of the triangle</param>
        /// <returns>an enumerable of all the vertexes</returns>
        public IEnumerable<Vector2> GetTriangleVertextesAround()
        {
            IEnumerable<Vector2> verts = new Vector2[] {
                new Vector3(-.5f,-1/(Mathf.Sqrt(3) * 2)),
                new Vector3(  0f, 1/Mathf.Sqrt(3)),
                new Vector3( .5f, -1/(Mathf.Sqrt(3) * 2)) };
            if (R)
            {
                var rotation = Quaternion.Euler(0, 0, -60);
                verts = verts.Select(x => (Vector2)(rotation * x));
            }
            Vector2 myPos = ToPositionInPlane();
            return verts.Select(x => x + myPos);
        }

        public Bounds GetRawBounds(float sideLength, Matrix4x4 systemTransform)
        {
            var position = systemTransform.MultiplyPoint3x4((Vector2)ToPositionInPlane());
            var boundingBoxSizeEstimate = Vector3.one * 2f / Mathf.Sqrt(3);
            return new Bounds(position, boundingBoxSizeEstimate * sideLength);
        }
        public static int[] GetTileTriangleIDs()
        {
            return new int[]
            {
                0, 1, 2
            };
        }

        public static TriangleCoordinate AtOrigin()
        {
            return new TriangleCoordinate(0, 0, false);
        }

        public static TriangleCoordinate operator +(TriangleCoordinate a, TriangleCoordinate b)
        {
            return new TriangleCoordinate(a.u + b.u, a.v + b.v, a.R || b.R);
        }

        public override int GetHashCode()
        {
            var coords = (u << 16) ^ (v);
            if (R)
            {
                return coords ^ (1 << 31);
            }
            return coords;
        }
        public override string ToString()
        {
            return $"u: {u}, v: {v}, R: {R}";
        }

        public override bool Equals(object obj)
        {
            if (obj is TriangleCoordinate coord)
            {
                return Equals(coord);
            }
            return false;
        }
        public bool Equals(TriangleCoordinate coord)
        {
            return coord.R == R && coord.u == u && coord.v == v;
        }
    }
}
