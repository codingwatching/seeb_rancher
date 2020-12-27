using Assets.Tiling.SquareCoords;
using Assets.Tiling.TriangleCoords;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using UnityEngine;

namespace Assets.Tiling
{

    /// <summary>
    /// Responsible only for representing a range of <see cref="ICoordinate"/>s
    ///     Is also responsible for returning information about the topology of all of the tiles within this range
    ///     This is used when detecting if it overlaps with tiles in a TileMap which may not use the same coordinate system or tile shape
    /// Used as a guidline to make sure all the methods used by UniversalCoordinateRange are there
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICoordinateRange<T> : IEnumerable<T> where T : struct, IBaseCoordinateType
    {
        /// <summary>
        /// bounds in-plane of the coordinates
        /// </summary>
        /// <returns></returns>
        IEnumerable<Vector2> BoundingPolygon();
        IEnumerable<T> BoundingCoordinates();
        int[] BoundingPolyTriangles { get; }

        bool ContainsCoordinate(UniversalCoordinate coordinate);

        bool ContainsCoordinate(T coordinate);

        int TotalCoordinateContents();
        T AtIndex(int index);
    }
    public enum CoordinateRangeType : short
    {
        INVALID = 0,
        TRIANGLE,
        TRIANGLE_RHOMBOID,
        RECTANGLE
    }

    [Serializable]
    [StructLayout(LayoutKind.Explicit)] // total size: 28bytes
    public struct UniversalCoordinateRange : ISerializable, IEquatable<UniversalCoordinateRange>
    {
        // all data views should take up no more than 3 longs, no more that 24 bytes
        [FieldOffset(0)] private long rangeDataPartOne;
        [FieldOffset(8)] private long rangeDataPartTwo;
        [FieldOffset(16)] private long rangeDataPartThree;

        [FieldOffset(0)] public TriangleTriangleCoordinateRange triangleDataView;
        [FieldOffset(0)] public TriangleRhomboidCoordinateRange triangeRhomboidDataView;
        [FieldOffset(0)] public RectCoordinateRange rectangleDataView;

        // TODO: Add plane index data!!
        [FieldOffset(24)] public CoordinateRangeType rangeType;
        [FieldOffset(26)] public short CoordinatePlaneID;

        public CoordinateType CoordinateType
        {
            get
            {
                switch (rangeType)
                {
                    case CoordinateRangeType.TRIANGLE:
                        return CoordinateType.TRIANGLE;
                    case CoordinateRangeType.TRIANGLE_RHOMBOID:
                        return CoordinateType.TRIANGLE;
                    case CoordinateRangeType.RECTANGLE:
                        return CoordinateType.SQUARE;
                    default:
                        return CoordinateType.INVALID;
                }
            }
        }

        public bool IsValid => rangeType != CoordinateRangeType.INVALID;

        public bool Equals(UniversalCoordinateRange other)
        {
            if (other.rangeType != rangeType || other.CoordinatePlaneID != CoordinatePlaneID)
            {
                return false;
            }
            switch (rangeType)
            {
                case CoordinateRangeType.TRIANGLE:
                    return triangleDataView.Equals(other.triangleDataView);
                case CoordinateRangeType.TRIANGLE_RHOMBOID:
                    return triangeRhomboidDataView.Equals(other.triangleDataView);
                case CoordinateRangeType.RECTANGLE:
                    return rectangleDataView.Equals(other.triangleDataView);
                default:
                    return false;
            }
        }

        public int TotalCoordinateContents()
        {
            switch (rangeType)
            {
                case CoordinateRangeType.TRIANGLE:
                    return triangleDataView.TotalCoordinateContents();
                case CoordinateRangeType.TRIANGLE_RHOMBOID:
                    return triangeRhomboidDataView.TotalCoordinateContents();
                case CoordinateRangeType.RECTANGLE:
                    return rectangleDataView.TotalCoordinateContents();
                default:
                    return -1;
            }
        }

        public bool ContainsCoordinate(UniversalCoordinate coordiante)
        {
            if (coordiante.CoordinatePlaneID != CoordinatePlaneID)
            {
                return false;
            }
            switch (rangeType)
            {
                case CoordinateRangeType.TRIANGLE:
                    return triangleDataView.ContainsCoordinate(coordiante);
                case CoordinateRangeType.TRIANGLE_RHOMBOID:
                    return triangeRhomboidDataView.ContainsCoordinate(coordiante);
                case CoordinateRangeType.RECTANGLE:
                    return rectangleDataView.ContainsCoordinate(coordiante);
                default:
                    return false;
            }
        }

        /// <summary>
        /// </summary>
        /// <returns>a list of points which form the borders of this range</returns>
        public IEnumerable<Vector2> BoundingPolygon()
        {
            switch (rangeType)
            {
                case CoordinateRangeType.TRIANGLE:
                    return triangleDataView.BoundingPolygon();
                case CoordinateRangeType.TRIANGLE_RHOMBOID:
                    return triangeRhomboidDataView.BoundingPolygon();
                case CoordinateRangeType.RECTANGLE:
                    return rectangleDataView.BoundingPolygon();
                default:
                    return null;
            }
        }

        /// <summary>
        /// These coordinates come back in the same order as the bounding polygon call, and will match up 1 to 1
        /// </summary>
        /// <returns>A list of coordinates at the most extreme points on this range</returns>
        public IEnumerable<UniversalCoordinate> BoundingCoordinates()
        {
            var planeIndexLambdaCapture = CoordinatePlaneID;
            switch (rangeType)
            {
                case CoordinateRangeType.TRIANGLE:
                    return triangleDataView.BoundingCoordinates().Select(coord => UniversalCoordinate.From(coord, planeIndexLambdaCapture));
                case CoordinateRangeType.TRIANGLE_RHOMBOID:
                    return triangeRhomboidDataView.BoundingCoordinates().Select(coord => UniversalCoordinate.From(coord, planeIndexLambdaCapture));
                case CoordinateRangeType.RECTANGLE:
                    return rectangleDataView.BoundingCoordinates().Select(coord => UniversalCoordinate.From(coord, planeIndexLambdaCapture));
                default:
                    return null;
            }
        }

        public int BoundingVertexCount()
        {
            switch (rangeType)
            {
                case CoordinateRangeType.TRIANGLE:
                    return 3;
                case CoordinateRangeType.TRIANGLE_RHOMBOID:
                    return 4;
                case CoordinateRangeType.RECTANGLE:
                    return 4;
                default:
                    return -1;
            }
        }

        public int[] BoundingPolyTriangles()
        {
            switch (rangeType)
            {
                case CoordinateRangeType.TRIANGLE:
                    return triangleDataView.BoundingPolyTriangles;
                case CoordinateRangeType.TRIANGLE_RHOMBOID:
                    return triangeRhomboidDataView.BoundingPolyTriangles;
                case CoordinateRangeType.RECTANGLE:
                    return rectangleDataView.BoundingPolyTriangles;
                default:
                    return null;
            }
        }

        public IEnumerable<UniversalCoordinate> GetUniversalCoordinates()
        {
            var planeIndexLambdaCapture = CoordinatePlaneID;
            switch (rangeType)
            {
                case CoordinateRangeType.TRIANGLE:
                    return triangleDataView.Select(coord => UniversalCoordinate.From(coord, planeIndexLambdaCapture));
                case CoordinateRangeType.TRIANGLE_RHOMBOID:
                    return triangeRhomboidDataView.Select(coord => UniversalCoordinate.From(coord, planeIndexLambdaCapture));
                case CoordinateRangeType.RECTANGLE:
                    return rectangleDataView.Select(coord => UniversalCoordinate.From(coord, planeIndexLambdaCapture));
                default:
                    return null;
            }
        }

        public UniversalCoordinate AtIndex(int index)
        {
            switch (rangeType)
            {
                case CoordinateRangeType.TRIANGLE:
                    return UniversalCoordinate.From(triangleDataView.AtIndex(index), CoordinatePlaneID);
                case CoordinateRangeType.TRIANGLE_RHOMBOID:
                    return UniversalCoordinate.From(triangeRhomboidDataView.AtIndex(index), CoordinatePlaneID);
                case CoordinateRangeType.RECTANGLE:
                    return UniversalCoordinate.From(rectangleDataView.AtIndex(index), CoordinatePlaneID);
                default:
                    return default;
            }
        }

        #region static constructors
        public static UniversalCoordinateRange From(TriangleTriangleCoordinateRange b, short planeId)
        {
            return new UniversalCoordinateRange
            {
                triangleDataView = b,
                rangeType = CoordinateRangeType.TRIANGLE,
                CoordinatePlaneID = planeId
            };
        }
        public static UniversalCoordinateRange From(TriangleRhomboidCoordinateRange b, short planeId)
        {
            return new UniversalCoordinateRange
            {
                triangeRhomboidDataView = b,
                rangeType = CoordinateRangeType.TRIANGLE_RHOMBOID,
                CoordinatePlaneID = planeId
            };
        }
        public static UniversalCoordinateRange From(RectCoordinateRange b, short planeId)
        {
            return new UniversalCoordinateRange
            {
                rectangleDataView = b,
                rangeType = CoordinateRangeType.RECTANGLE,
                CoordinatePlaneID = planeId
            };
        }

        /// <summary>
        /// Returns the default range used for each coordinate type; which uses two points to define its bounds
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static UniversalCoordinateRange From(UniversalCoordinate a, UniversalCoordinate b)
        {
            if (a.CoordinateMembershipData != b.CoordinateMembershipData)
            {
                return default;
            }
            switch (a.type)
            {
                case CoordinateType.TRIANGLE:
                    return From(TriangleRhomboidCoordinateRange.FromCoordsInclusive(a.triangleDataView, b.triangleDataView), a.CoordinatePlaneID);
                case CoordinateType.SQUARE:
                    return From(RectCoordinateRange.FromCoordsInclusive(a.squareDataView, b.squareDataView), a.CoordinatePlaneID);
                default:
                    return default;
            }
        }
        #endregion

        #region Serialization
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("data1", rangeDataPartOne);
            info.AddValue("data2", rangeDataPartTwo);
            info.AddValue("data3", rangeDataPartThree);
            info.AddValue("rangeType", (short)rangeType);
            info.AddValue("planeId", CoordinatePlaneID);
        }

        // The special constructor is used to deserialize values.
        private UniversalCoordinateRange(SerializationInfo info, StreamingContext context)
        {
            triangleDataView = default;
            triangeRhomboidDataView = default;
            rectangleDataView = default;
            rangeType = CoordinateRangeType.INVALID;

            rangeDataPartOne = info.GetInt64("data1");
            rangeDataPartTwo = info.GetInt64("data2");
            rangeDataPartThree = info.GetInt64("data3");
            rangeType = (CoordinateRangeType)info.GetInt16("rangeType");
            CoordinatePlaneID = info.GetInt16("planeId");
        }
        #endregion

        public override string ToString()
        {
            switch (rangeType)
            {
                case CoordinateRangeType.TRIANGLE:
                    return triangleDataView.ToString();
                case CoordinateRangeType.TRIANGLE_RHOMBOID:
                    return triangeRhomboidDataView.ToString();
                case CoordinateRangeType.RECTANGLE:
                    return rectangleDataView.ToString();
                default:
                    return "Invalid Range";
            }
        }
    }
}
