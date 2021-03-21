using Dman.Tiling;
using Dman.Tiling.SquareCoords;
using Dman.Tiling.TileSets;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.GreenhouseLoader
{
    /// <summary>
    /// controls the pattern in which all the tiles of the greenhouse are created
    /// </summary>
    [CreateAssetMenu(fileName = "FloorTiles", menuName = "Greenhouse/FloorTiles", order = 1)]
    public class FloorTiles : ScriptableObject
    {
        public RectCoordinateRange floorSize;
        public RectCoordinateRange dirtPlanterSize;

        public TileType dirtTile;
        public TileType dirtEdgeTile;

        public TileType floorTile;
        public TileType floorEdgeTile;

        public TileType doorTile;

        public Dictionary<UniversalCoordinate, TileType> GenerateFloorPlan()
        {
            var tiles = new Dictionary<UniversalCoordinate, TileType>();
            GenerateRangeAndBorders(floorSize, floorTile, floorEdgeTile, tiles);
            GenerateRangeAndBorders(dirtPlanterSize, dirtTile, dirtEdgeTile, tiles);

            var doorCoordinate = new SquareCoordinate(floorSize.rows, floorSize.cols / 2) + floorSize.coord0;
            tiles[UniversalCoordinate.From(doorCoordinate)] = doorTile;

            return tiles;
        }

        private void GenerateRangeAndBorders(RectCoordinateRange tileRange, TileType centerTile, TileType borderTile, Dictionary<UniversalCoordinate, TileType> tiles)
        {
            foreach (var coordinate in tileRange)
            {
                tiles[UniversalCoordinate.From(coordinate)] = centerTile;
            }
            foreach (var coordinate in GetBorders(tileRange))
            {
                tiles[UniversalCoordinate.From(coordinate)] = borderTile;
            }
        }


        private IEnumerable<SquareCoordinate> GetBorders(RectCoordinateRange rangeToBorder)
        {
            var rootForBorder = rangeToBorder.coord0 + new SquareCoordinate(-1, -1);
            for (int column = 0; column < rangeToBorder.cols + 2; column++)
            {
                yield return new SquareCoordinate(0, column) + rootForBorder;
                yield return new SquareCoordinate(rangeToBorder.rows + 1, column) + rootForBorder;
            }
            for (int row = 1; row < rangeToBorder.rows + 1; row++)
            {
                yield return new SquareCoordinate(row, 0) + rootForBorder;
                yield return new SquareCoordinate(row, rangeToBorder.cols + 1) + rootForBorder;
            }
        }
    }
}
