using System.Collections.Generic;
using System.Linq;

namespace Simulation.Tiling.HexCoords
{
    public class VoroniTilingMapper
    {
        /// <summary>
        /// evaluate a voroni map for a given list of center points
        /// </summary>
        /// <param name="centerPoints"></param>
        /// <param name="minimumBound">the rectangular minimum of the space to evaluate the map inside</param>
        /// <param name="maximumBound">the rectangular maximum of the space to evaluate the map inside</param>
        /// <returns></returns>
        public static int[][] SetupVoroniMap(
            IList<AxialCoordinate> centerPoints,
            OffsetCoordinate minimumBound,
            OffsetCoordinate maximumBound)
        {
            var validWidth = maximumBound.column - minimumBound.column;
            var validHeight = maximumBound.row - minimumBound.row;
            int[][] owners = new int[validHeight][];
            for (var row = 0; row < validHeight; row++)
            {
                owners[row] = new int[validWidth];
                for (var col = 0; col < validWidth; col++)
                {
                    owners[row][col] = -1;
                }
            }

            var minimumAxial = minimumBound.ToAxial();

            var ringDistance = -1;
            var addedNewPoint = true;

            while (addedNewPoint)
            {
                ringDistance++;
                addedNewPoint = false;
                for (var centerIndex = 0; centerIndex < centerPoints.Count; centerIndex++)
                {
                    var centerPoint = centerPoints[centerIndex];

                    var ring = HexCoordinateSystem
                        .GetRing(centerPoint, ringDistance)
                        .Select(ringPoint => (ringPoint - minimumAxial).ToOffset());
                    foreach (var ringPoint in ring)
                    {
                        if (0 <= ringPoint.column && ringPoint.column < validWidth &&
                           0 <= ringPoint.row && ringPoint.row < validHeight)
                        {
                            var currentOwner = owners[ringPoint.row][ringPoint.column];
                            if (currentOwner == -1)
                            {
                                owners[ringPoint.row][ringPoint.column] = centerIndex;
                                addedNewPoint = true;
                            }
                        }
                    }
                }
            }

            return owners;
        }
    }
}
