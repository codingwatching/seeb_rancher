using Assets.Scripts.Utilities;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Scripts.Tiling.TileSets
{
    public abstract class TileType : IDableObject
    {
        public int uniqueID;
        public override void AssignId(int myNewID)
        {
            uniqueID = myNewID;
        }

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
            float2 offsetOnFloor,
            Transform parentTransform,
            UniversalCoordinateSystemMembers members);
    }
}
