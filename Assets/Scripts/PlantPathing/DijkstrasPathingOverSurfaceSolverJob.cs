using Assets.Scripts.GreenhouseLoader;
using Dman.LSystem.SystemRuntime.VolumetricData;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using static Assets.Scripts.PlantPathing.DijkstrasPathingSolverJob;

namespace Assets.Scripts.PlantPathing
{

    public struct DijkstrasPathingOverSurfaceSolverProprocessingJob : IJob
    {
        [ReadOnly]
        public NativeArray<float> nodeCostAdjustmentVoxels;
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
                        var voxelId = voxelLayout.GetDataIndexFromCoordinates(new Vector3Int(x, y, z));
                        var voxelCost = nodeCostAdjustmentVoxels[voxelId];
                        surfaceCost += voxelCost;
                    }

                    var surfaceCoordinate = new Vector2Int(x, z);
                    var surfaceId = voxelLayout.SurfaceGetDataIndexFromCoordinates(surfaceCoordinate);
                    calculatedSurfaceWeights[surfaceId] = surfaceCost;
                }
            }
        }
    }

    /// <summary>
    /// solves and caches a pathing grid, which paths to a specific target voxel using Dijkstra's algorithm
    /// </summary>
    public struct DijkstrasPathingOverSurfaceSolverJob : IJob
    {
        public VolumetricWorldVoxelLayout voxelLayout;
        [ReadOnly]
        public NativeArray<float> calculatedSurfaceWeights;

        // working data
        public NativeArray<int> parentNodePointers;
        public NativeArray<PathingNodeTmpData> tmpPathingData;

        public NativeQueue<int> frontNodes;

        public Vector2Int targetSurface;
        public float baseTravelWeight;

        public NativeArray<bool> failedWithInfiniteLoop;

        public void Execute()
        {
            var firstIndex = voxelLayout.SurfaceGetDataIndexFromCoordinates(targetSurface);
            var firstData = tmpPathingData[firstIndex];
            firstData.totalTravelCost = 0;
            tmpPathingData[firstIndex] = firstData;

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

        private void Visit(int index)
        {
            var data = tmpPathingData[index];
            var coordinate = voxelLayout.SurfaceGetCoordinatesFromDataIndex(index);

            CheckNeighbor(data, index, coordinate + new Vector2Int(1, 0));
            CheckNeighbor(data, index, coordinate + new Vector2Int(-1, 0));
            CheckNeighbor(data, index, coordinate + new Vector2Int(0, 1));
            CheckNeighbor(data, index, coordinate + new Vector2Int(0, -1));
        }

        private void CheckNeighbor(PathingNodeTmpData sourceNode, int sourceIndex, Vector2Int targetCoordinate)
        {
            var neighborIndex = this.voxelLayout.SurfaceGetDataIndexFromCoordinates(targetCoordinate);
            if (neighborIndex < 0)
            {
                return;
            }
            var neighborNode = tmpPathingData[neighborIndex];
            var travelCost = calculatedSurfaceWeights[neighborIndex] + baseTravelWeight;
            var newWeight = travelCost + sourceNode.totalTravelCost;
            if (newWeight < neighborNode.totalTravelCost)
            {
                neighborNode.totalTravelCost = newWeight;
                tmpPathingData[neighborIndex] = neighborNode;
                parentNodePointers[neighborIndex] = sourceIndex;
                frontNodes.Enqueue(neighborIndex);
            }
        }
    }
}
