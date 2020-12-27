using Assets.Scripts.Tiling.TileSets;
using Assets.Tiling;
using Assets.Tiling.SquareCoords;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.GreenhouseLoader
{
    [CreateAssetMenu(fileName = "FloorTiles", menuName = "Greenhouse/FloorTiles", order = 1)]
    public class FloorTiles : ScriptableObject
    {
        public RectCoordinateRange floorSize;
        public TileType floorTile;
        public TileType wallTile;

        public TileMembersSaveObject GenerateFloorPlan()
        {
            var tiles = new Dictionary<UniversalCoordinate, TileType>();

            foreach (var coordinate in floorSize)
            {
                tiles[UniversalCoordinate.From(coordinate)] = floorTile;
            }

            return TileMembersSaveObject.FromTileTypeDictionary(tiles);
        }
    }
}
