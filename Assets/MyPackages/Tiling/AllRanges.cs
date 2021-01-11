using Dman.ObjectSets;
using Unity.Mathematics;
using UnityEngine;

namespace Dman.Tiling
{
    [CreateAssetMenu(fileName = "AllRanges", menuName = "Tiling/AllRanges", order = 1)]
    public class AllRanges : UniqueObjectRegistryWithAccess<RangePositioner>
    {
        public float2 TransformCoordinate(UniversalCoordinate coordinate)
        {
            var range = allObjects[coordinate.CoordinatePlaneID];
            return range.TransformCoordinate(coordinate);
        }
    }
}
