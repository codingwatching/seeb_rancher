using Dman.ObjectSets;
using Unity.Mathematics;
using UnityEngine;

namespace Dman.Tiling.TileSets
{
    public abstract class TileType : IDableObject
    {
        protected GameObject BasicCreateTile(
            float2 offsetOnFloor,
            GameObject tilePrefab,
            Transform parentTransform)
        {
            var newTile = Instantiate(tilePrefab, parentTransform);
            newTile.transform.position = new Vector3(offsetOnFloor.x, newTile.transform.position.y, offsetOnFloor.y);
            return newTile;
        }

        public abstract GameObject CreateTile(
            UniversalCoordinate coordinate,
            float2 offsetOnFloor,
            Transform parentTransform,
            UniversalCoordinateSystemMembers members);
    }
}
