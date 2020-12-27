using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Simulation.Tiling.HexCoords
{
    public class HexCoordinateSystem// : ICoordinateSystem<AxialCoordinate>
    {
        public float hexRadius;

        private readonly Vector2 qBasis = new Vector2(3f / 2f, -Mathf.Sqrt(3) / 2f);
        private readonly Vector2 rBasis = new Vector2(0, -Mathf.Sqrt(3));

        public HexCoordinateSystem(float hexRadius)
        {
            this.hexRadius = hexRadius;
        }

        /// <summary>
        /// Translate a tile map coordinate to a standard "real" position. this is not scaled based
        ///     on the size of the hexes. only use it for calculations that need to know about
        ///     relative positioning of hex members as opposed to real positioning
        /// </summary>
        /// <param name="offsetCoordinates"></param>
        /// <returns></returns>
        public Vector2 TileMapToRelative(AxialCoordinate axial)
        {
            var x = qBasis.x * axial.q;
            var y = qBasis.y * axial.q + rBasis.y * axial.r;
            return new Vector2(x, y);
        }
        public Vector2 TileMapToReal(AxialCoordinate coordinate)
        {
            return TileMapToRelative(coordinate) * hexRadius;
        }

        public CubeCoordinate RelativeToTileMap(Vector2 relativePosition)
        {
            var cubicFloating = ConvertSizeScaledPointToFloatingCubic(relativePosition);
            cubicFloating.z = -cubicFloating.x - cubicFloating.y;
            return new CubeCoordinate(cubicFloating);
        }
        public CubeCoordinate RealToTileMap(Vector2 realPosition)
        {
            var relativePositioning = realPosition / hexRadius;
            return RelativeToTileMap(relativePositioning);
        }

        public static IEnumerable<AxialCoordinate> GetPositionsWithinJumpDistance(AxialCoordinate origin, int jumpDistance)
        {
            for (var q = -jumpDistance; q <= jumpDistance; q++)
            {
                var sliceStart = Mathf.Max(-jumpDistance, -q - jumpDistance);
                var sliceEnd = Mathf.Min(jumpDistance, -q + jumpDistance);
                for (var r = sliceStart; r <= sliceEnd; r++)
                {
                    yield return new AxialCoordinate(q, r) + origin;
                }
            }
        }

        public IEnumerable<AxialCoordinate> GetPositionsSpiralingAround(AxialCoordinate origin)
        {
            return InternalSpiraling(origin).SelectMany(x => x);
        }

        private IEnumerable<IEnumerable<AxialCoordinate>> InternalSpiraling(AxialCoordinate origin)
        {
            var distance = 0;
            while (true)
            {
                yield return GetRing(origin, distance);
                distance++;
            }
        }

        public static IEnumerable<AxialCoordinate> GetRing(AxialCoordinate origin, int distance)
        {
            if (distance == 0)
            {
                yield return origin;
            }

            var currentPointInRing = origin + (AxialCoordinate.GetDirection(4) * distance);
            for (var directionIndex = 0; directionIndex < 6; directionIndex++)
            {
                for (var i = 0; i < distance; i++)
                {
                    yield return currentPointInRing;
                    currentPointInRing = currentPointInRing.GetNeighbor(directionIndex);
                }
            }
        }

        public IEnumerable<AxialCoordinate> GetRouteGenerator(AxialCoordinate origin, AxialCoordinate destination)
        {
            var currentTileMapPos = new AxialCoordinate(origin.q, origin.r);
            var myDest = new AxialCoordinate(destination.q, destination.r);
            var iterations = 0;
            while (!currentTileMapPos.Equals(myDest))
            {
                var realWorldVectorToDest = TileMapToRelative(myDest)
                    - TileMapToRelative(currentTileMapPos);

                var nextMoveVector = GetClosestMatchingValidMove(realWorldVectorToDest);

                currentTileMapPos = currentTileMapPos + nextMoveVector;

                yield return currentTileMapPos;
                iterations++;
                if (iterations > 1000)
                {
                    throw new Exception("too many loop brooother");
                }
            }
        }

        private AxialCoordinate GetClosestMatchingValidMove(Vector2 worldSpaceDestinationVector)
        {
            var angle = Vector2.SignedAngle(Vector2.right, worldSpaceDestinationVector);
            if (0 <= angle && angle < 60)
            {
                return new AxialCoordinate(1, -1); ;
            }
            if (60 <= angle && angle < 120)
            {
                return new AxialCoordinate(0, -1);
            }
            if (120 <= angle && angle <= 180)
            {
                return new AxialCoordinate(-1, 0);
            }
            if (-180 <= angle && angle < -120)
            {
                return new AxialCoordinate(-1, 1);
            }
            if (-120 <= angle && angle < -60)
            {
                return new AxialCoordinate(0, 1);
            }
            if (-60 <= angle && angle < 0)
            {
                return new AxialCoordinate(1, 0);
            }
            throw new Exception($"error in angle matching {angle}");
        }

        private Vector3 ConvertSizeScaledPointToFloatingCubic(Vector2 point)
        {
            var q = 2f / 3f * point.x;
            var r = -1f / 3f * point.x + Mathf.Sqrt(3) / 3f * point.y;
            var cubicCoords = new Vector3(q, r, 0);
            return cubicCoords;
        }


        public IEnumerable<AxialCoordinate> Neighbors(AxialCoordinate coordinate)
        {
            return HexCoordinateSystem.GetRing(coordinate, 1);
        }

        public Vector2 ToRealPosition(AxialCoordinate coordinate)
        {
            return TileMapToReal(coordinate);
        }

        public AxialCoordinate FromRealPosition(Vector2 realWorldPos)
        {
            return RealToTileMap(realWorldPos).ToAxial();
        }

        public AxialCoordinate DefaultCoordinate()
        {
            return new AxialCoordinate(0, 0);
        }

        public float HeuristicDistance(AxialCoordinate origin, AxialCoordinate destination)
        {
            return (ToRealPosition(origin) - ToRealPosition(destination)).sqrMagnitude;
        }
    }
}
