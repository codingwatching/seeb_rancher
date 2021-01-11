using Unity.Mathematics;
using UnityEngine;

namespace Dman.Tiling.TileSets
{
    [CreateAssetMenu(fileName = "RotationallySymmetricTileType", menuName = "Greenhouse/RotationallySymmetricTileType", order = 2)]
    public class RotationallySymmetricTileType : TileType
    {
        public GameObject tileModelPrefab;

        public override GameObject CreateTile(
            UniversalCoordinate coordinate,
            float2 offsetOnFloor,
            Transform parentTransform,
            UniversalCoordinateSystemMembers members)
        {
            var newTile = base.BasicCreateTile(offsetOnFloor, tileModelPrefab, parentTransform);
            newTile.transform.localRotation *= Quaternion.Euler(0, 0, 90 * UnityEngine.Random.Range(0, 3));
            return newTile;
        }
    }
}
