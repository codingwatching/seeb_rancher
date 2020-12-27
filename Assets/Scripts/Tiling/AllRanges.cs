using Assets.Scripts.Utilities;
using Assets.Tiling;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Scripts.Tiling
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
