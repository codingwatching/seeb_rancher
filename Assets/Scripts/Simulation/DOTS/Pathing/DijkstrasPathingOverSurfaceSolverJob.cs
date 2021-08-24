using Dman.LSystem.SystemRuntime.VolumetricData;
using Dman.LSystem.SystemRuntime.VolumetricData.NativeVoxels;
using Environment;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using static Simulation.DOTS.Pathing.DijkstrasPathingSolverJob;

namespace Simulation.DOTS.Pathing
{

    [BurstCompile]
    public struct DijkstrasPathingOverSurfaceSolverProprocessingJob : IJob
    {
        [ReadOnly]
        public VoxelWorldVolumetricLayerData layerData;
        public int costAdjustmentLayer;
        public NativeArray<float> calculatedSurfaceWeights;

        public PerlinSamplerNativeCompatable terrainSampler;

        public VolumetricWorldVoxelLayout voxelLayout;
        public float patherHeight;

        public void Execute()
        {
            for (int x = 0; x < voxelLayout.worldResolution.x; x++)
            {
                for (int z = 0; z < voxelLayout.worldResolution.z; z++)
                {
                    var terrainHeight = terrainSampler.SampleNoise(new Vector2(x + 0.5f, z + 0.5f));
                    var maxVoxelHeight = terrainHeight + patherHeight;

                    var minVoxel = (int)math.max(math.floor(terrainHeight), 0);
                    var maxVoxel = (int)math.ceil(maxVoxelHeight);

                    var surfaceCost = 0f;
                    for (int y = minVoxel; y <= maxVoxel && y < voxelLayout.worldResolution.y; y++)
                    {
                        var voxelId = voxelLayout.GetVoxelIndexFromCoordinates(new Vector3Int(x, y, z));
                        var voxelCost = layerData[voxelId, 0];
                        surfaceCost += voxelCost;
                    }

                    var surfaceCoordinate = new Vector2Int(x, z);
                    var surfaceId = voxelLayout.SurfaceGetTileIndexFromCoordinates(surfaceCoordinate);
                    calculatedSurfaceWeights[surfaceId.Value] = surfaceCost;
                }
            }
        }
    }

    /// <summary>
    /// solves and caches a pathing grid, which paths to a specific target voxel using Dijkstra's algorithm
    /// </summary>
    [BurstCompile]
    public struct DijkstrasPathingOverSurfaceSolverJob : IJob
    {
        public VolumetricWorldVoxelLayout voxelLayout;
        [ReadOnly]
        public NativeArray<float> calculatedSurfaceWeights;

        // working data
        public NativeArray<int> parentNodePointers;
        public NativeArray<PathingNodeTmpData> tmpPathingData;

        public NativeQueue<TileIndex> frontNodes;

        public Vector2Int targetSurface;
        public float baseTravelWeight;

        public NativeArray<bool> failedWithInfiniteLoop;

        public void Execute()
        {
            var firstIndex = voxelLayout.SurfaceGetTileIndexFromCoordinates(targetSurface);
            var firstData = tmpPathingData[firstIndex.Value];
            firstData.totalTravelCost = 0;
            tmpPathingData[firstIndex.Value] = firstData;

            frontNodes.Enqueue(firstIndex);

            var safetyMaxIterationCheck = (int)Mathf.Pow(voxelLayout.worldResolution.x * voxelLayout.worldResolution.z, 1.5f);
            var totalVisitations = 0;
            while (frontNodes.Count > 0)
            {
                totalVisitations++;
                if (totalVisitations > safetyMaxIterationCheck)
                {
                    failedWithInfiniteLoop[0] = true;
                    return;
                }
                var nextNode = frontNodes.Dequeue();
                Visit(nextNode);
            }
        }

        private void Visit(TileIndex index)
        {
            var data = tmpPathingData[index.Value];
            var coordinate = voxelLayout.SurfaceGetCoordinatesFromTileIndex(index);

            CheckNeighbor(data, index, coordinate + new Vector2Int(1, 0));
            CheckNeighbor(data, index, coordinate + new Vector2Int(-1, 0));
            CheckNeighbor(data, index, coordinate + new Vector2Int(0, 1));
            CheckNeighbor(data, index, coordinate + new Vector2Int(0, -1));
        }

        private void CheckNeighbor(PathingNodeTmpData sourceNode, TileIndex sourceIndex, Vector2Int targetCoordinate)
        {
            var neighborIndex = this.voxelLayout.SurfaceGetTileIndexFromCoordinates(targetCoordinate);
            if (!neighborIndex.IsValid)
            {
                return;
            }
            var neighborNode = tmpPathingData[neighborIndex.Value];
            var travelCost = calculatedSurfaceWeights[neighborIndex.Value] + baseTravelWeight;
            var newWeight = travelCost + sourceNode.totalTravelCost;
            if (newWeight < neighborNode.totalTravelCost)
            {
                neighborNode.totalTravelCost = newWeight;
                tmpPathingData[neighborIndex.Value] = neighborNode;
                parentNodePointers[neighborIndex.Value] = sourceIndex.Value;
                frontNodes.Enqueue(neighborIndex);
            }
        }
    }
}
