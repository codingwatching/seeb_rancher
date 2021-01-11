using Dman.Tiling.SquareCoords;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace Dman.Tiling.TileSets
{
    enum NR // neighbor State
    {
        /// <summary>
        /// Set
        /// </summary>
        SET,
        /// <summary>
        /// Unset
        /// </summary>
        UNSET,
        /// <summary>
        /// Irrelivent
        /// </summary>
        IRRELIVENT
    }
    enum EdgingState
    {
        NONE,
        Flat,
        Corner,
        DoubleCorner,
        QuadCorner,
        SingleEdge
    }
    public enum EdgeRotation
    {
        RIGHT = 0,
        UP = 1,
        LEFT = 2,
        DOWN = 3
    }
    class EdgeMatch
    {
        public EdgeRotation rotation;
        public EdgingState targetState;
    }
    class EdgingStateChecker
    {
        public EdgingState TargetState { get; private set; }
        private NR[] matchingNeighbors;

        public EdgingStateChecker(EdgingState targetState, NR[] matchingNeighbors)
        {
            TargetState = targetState;
            this.matchingNeighbors = matchingNeighbors;
        }

        public EdgeMatch Matches(bool[] neighbors)
        {
            for (int offset = 0; offset < 4; offset++)
            {
                if (MatchAtOffset(neighbors, offset * 2))
                {
                    return new EdgeMatch
                    {
                        rotation = (EdgeRotation)(4 - offset),
                        targetState = TargetState
                    };
                }
            }
            return null;
        }

        private bool MatchAtOffset(bool[] neighbors, int offset)
        {
            var size = neighbors.Length;
            for (int i = 0; i < neighbors.Length; i++)
            {
                switch (matchingNeighbors[i])
                {
                    case NR.SET:
                        if (!neighbors[(i + offset) % size]) return false;
                        break;
                    case NR.UNSET:
                        if (neighbors[(i + offset) % size]) return false;
                        break;
                    case NR.IRRELIVENT:
                    default:
                        break;
                }
            }
            return true;
        }
    }

    [CreateAssetMenu(fileName = "SquareEdgeTileType", menuName = "Greenhouse/SquareEdgeTileType", order = 2)]
    public class SquareEdgeTileType : TileType
    {
        public GameObject flatEdgeTileModel;
        public GameObject cornerTileModel;
        public GameObject doubleCorner;
        public GameObject quadCorner;
        public GameObject singleEdge;

        public TileType typeToEdge;

        private SquareCoordinate[] NeighborsToCheck = new SquareCoordinate[]
        {
            SquareCoordinate.RIGHT,
            SquareCoordinate.RIGHT + SquareCoordinate.UP,
            SquareCoordinate.UP,
            SquareCoordinate.UP + SquareCoordinate.LEFT,
            SquareCoordinate.LEFT,
            SquareCoordinate.LEFT + SquareCoordinate.DOWN,
            SquareCoordinate.DOWN,
            SquareCoordinate.RIGHT + SquareCoordinate.DOWN
        };

        private EdgingStateChecker[] StateChecks = new EdgingStateChecker[]
        {
            new EdgingStateChecker(EdgingState.Flat,
                new NR[]
                {
                    NR.IRRELIVENT, NR.UNSET, NR.UNSET, NR.IRRELIVENT, NR.SET, NR.IRRELIVENT, NR.UNSET, NR.UNSET
                }),
            new EdgingStateChecker(EdgingState.Corner,
                new NR[]
                {
                    NR.UNSET, NR.SET, NR.UNSET, NR.UNSET, NR.UNSET, NR.UNSET, NR.UNSET, NR.UNSET
                })
        };

        private EdgeMatch GetEdgeMatchFromNeighbors(bool[] neighbors)
        {
            for (int i = 0; i < StateChecks.Length; i++)
            {
                var match = StateChecks[i].Matches(neighbors);
                if (match != null)
                {
                    return match;
                }
            }
            return null;
        }
        private GameObject GetEdgePrefabFromEdgeState(EdgeMatch edgingData)
        {
            switch (edgingData.targetState)
            {
                case EdgingState.Flat:
                    return flatEdgeTileModel;
                case EdgingState.Corner:
                    return cornerTileModel;
                case EdgingState.DoubleCorner:
                    return doubleCorner;
                case EdgingState.QuadCorner:
                    return quadCorner;
                case EdgingState.SingleEdge:
                    return singleEdge;
                case EdgingState.NONE:
                default:
                    return singleEdge;
            }
        }

        public static void RotateInstanceByRotatedMatch(EdgeRotation rotation, Transform transform)
        {
            transform.localRotation *= Quaternion.Euler(0, 0, 90 * ((int)rotation));
        }


        public override GameObject CreateTile(
            UniversalCoordinate coordinate,
            float2 offsetOnFloor,
            Transform parentTransform,
            UniversalCoordinateSystemMembers members)
        {
            var squareCoord = coordinate.squareDataView;
            var neighbors = NeighborsToCheck
                .Select(x => UniversalCoordinate.From(squareCoord + x, coordinate.CoordinatePlaneID))
                .Select(x => members.GetTileType(x) == typeToEdge)
                .ToArray();

            var edgeMatch = GetEdgeMatchFromNeighbors(neighbors);
            if (edgeMatch == null)
            {
                edgeMatch = new EdgeMatch
                {
                    rotation = EdgeRotation.RIGHT,
                    targetState = EdgingState.SingleEdge
                };
            }
            var edgeTile = GetEdgePrefabFromEdgeState(edgeMatch);

            var newTile = base.BasicCreateTile(offsetOnFloor, edgeTile, parentTransform);
            RotateInstanceByRotatedMatch(edgeMatch.rotation, newTile.transform);
            return newTile;
        }
    }
}
