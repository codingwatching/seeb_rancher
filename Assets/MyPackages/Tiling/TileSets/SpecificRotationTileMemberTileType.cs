using Unity.Mathematics;
using UnityEngine;

namespace Dman.Tiling.TileSets
{
    [CreateAssetMenu(fileName = "SpecificRotationTileType", menuName = "Greenhouse/SpecificRotationTileType", order = 2)]
    public class SpecificRotationTileMemberTileType : TileType
    {
        public GreenhouseMember tileModelPrefab;
        public EdgeRotation specificRotation;

        public override GameObject CreateTile(
            UniversalCoordinate coordinate,
            float2 offsetOnFloor,
            Transform parentTransform,
            UniversalCoordinateSystemMembers members)
        {
            var newTile = Instantiate(tileModelPrefab, parentTransform);
            newTile.SetPosition(coordinate);
            SquareEdgeTileType.RotateInstanceByRotatedMatch(specificRotation, newTile.transform);
            return newTile.gameObject;
        }
    }
}
