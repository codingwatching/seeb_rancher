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
        public TileType doorTile;

        public Dictionary<UniversalCoordinate, TileType> GenerateFloorPlan()
        {
            var tiles = new Dictionary<UniversalCoordinate, TileType>();

            foreach (var coordinate in floorSize)
            {
                tiles[UniversalCoordinate.From(coordinate)] = floorTile;
            }

            var doorCoordinate = new SquareCoordinate(floorSize.rows, floorSize.cols / 2) + floorSize.coord0;
            tiles[UniversalCoordinate.From(doorCoordinate)] = doorTile;
            foreach (var coordinate in GetBorders(floorSize))
            {
                var coord = UniversalCoordinate.From(coordinate);
                if (!tiles.ContainsKey(coord))
                    tiles[coord] = wallTile;
            }

            return tiles;
        }

        private IEnumerable<SquareCoordinate> GetBorders(RectCoordinateRange rangeToBorder)
        {
            var rootForBorder = floorSize.coord0 + new SquareCoordinate(-1, -1);
            for (int column = 0; column < floorSize.cols + 2; column++)
            {
                yield return new SquareCoordinate(0, column) + rootForBorder;
                yield return new SquareCoordinate(floorSize.rows + 1, column) + rootForBorder;
            }
            for (int row = 1; row < floorSize.rows + 1; row++)
            {
                yield return new SquareCoordinate(row, 0) + rootForBorder;
                yield return new SquareCoordinate(row, floorSize.cols + 1) + rootForBorder;
            }
        }
    }
}
