using Dman.ObjectSets;
using Unity.Mathematics;
using UnityEngine;

namespace Dman.Tiling
{
    [CreateAssetMenu(fileName = "TilePositioner", menuName = "Tiling/TilePositioner", order = 1)]
    public class RangePositioner : IDableObject
    {
        public float2 coordinateScaling;
        public CoordinateType coordinateType;
        public float2 TransformCoordinate(UniversalCoordinate coordinate)
        {
            var position = coordinate.ToPositionInPlane();
            return position * coordinateScaling;
        }
        public UniversalCoordinate InverseTransformCoordinate(float2 position)
        {
            var localOnPlane = position / coordinateScaling;
            return UniversalCoordinate.FromPositionInPlane(localOnPlane, coordinateType, (short)myId);
        }
    }
}
