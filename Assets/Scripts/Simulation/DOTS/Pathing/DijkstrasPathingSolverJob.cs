using Dman.LSystem.SystemRuntime.VolumetricData;
using Dman.LSystem.SystemRuntime.VolumetricData.NativeVoxels;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Simulation.DOTS.Pathing
{

    [BurstCompile]
    public struct DijkstrasPathingSolverInitializerJob : IJobParallelFor
    {
        public NativeArray<int> parentNodePointers;
        public NativeArray<DijkstrasPathingSolverJob.PathingNodeTmpData> tmpPathingData;
        public void Execute(int index)
        {
            parentNodePointers[index] = VoxelIndex.Invalid.Value;
            tmpPathingData[index] = DijkstrasPathingSolverJob.PathingNodeTmpData.DefaultData();
        }
    }

    /// <summary>
    /// solves and caches a pathing grid, which paths to a specific target voxel using Dijkstra's algorithm
    /// </summary>
    [BurstCompile]
    public struct DijkstrasPathingSolverJob : IJob
    {
        public VolumetricWorldVoxelLayout voxelLayout;
        [ReadOnly]
        public VoxelWorldVolumetricLayerData layerData;
        public int costAdjustmentLayer;

        // working data
        public NativeArray<int> parentNodePointers;
        public NativeArray<PathingNodeTmpData> tmpPathingData;

        public NativeQueue<VoxelIndex> frontNodes;

        public Vector3Int targetVoxel;
        public float baseTravelWeight;

        public NativeArray<bool> failedWithInfiniteLoop;

        public void Execute()
        {
            var firstIndex = voxelLayout.GetVoxelIndexFromCoordinates(targetVoxel);
            var firstData = tmpPathingData[firstIndex.Value];
            firstData.totalTravelCost = 0;
            tmpPathingData[firstIndex.Value] = firstData;

            frontNodes.Enqueue(firstIndex);

            var safetyMaxIterationCheck = (int)Mathf.Pow(voxelLayout.totalVoxels, 1.5f);
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

        private void Visit(VoxelIndex index)
        {
            var data = tmpPathingData[index.Value];
            var coordinate = voxelLayout.GetCoordinatesFromVoxelIndex(index);

            CheckNeighbor(data, index, coordinate + new Vector3Int(1, 0, 0));
            CheckNeighbor(data, index, coordinate + new Vector3Int(-1, 0, 0));
            CheckNeighbor(data, index, coordinate + new Vector3Int(0, 1, 0));
            CheckNeighbor(data, index, coordinate + new Vector3Int(0, -1, 0));
            CheckNeighbor(data, index, coordinate + new Vector3Int(0, 0, 1));
            CheckNeighbor(data, index, coordinate + new Vector3Int(0, 0, -1));
        }

        private void CheckNeighbor(PathingNodeTmpData sourceNode, VoxelIndex sourceIndex, Vector3Int targetCoordinate)
        {
            var neighborIndex = voxelLayout.GetVoxelIndexFromCoordinates(targetCoordinate);
            if (!neighborIndex.IsValid)
            {
                return;
            }
            var neighborNode = tmpPathingData[neighborIndex.Value];
            var travelCost = layerData[neighborIndex, costAdjustmentLayer] + baseTravelWeight;
            var newWeight = travelCost + sourceNode.totalTravelCost;
            if (newWeight < neighborNode.totalTravelCost)
            {
                neighborNode.totalTravelCost = newWeight;
                tmpPathingData[neighborIndex.Value] = neighborNode;
                parentNodePointers[neighborIndex.Value] = sourceIndex.Value;
                frontNodes.Enqueue(neighborIndex);
            }
        }

        public struct PathingNodeTmpData
        {
            public float totalTravelCost;

            public static PathingNodeTmpData DefaultData()
            {
                return new PathingNodeTmpData
                {
                    totalTravelCost = float.PositiveInfinity
                };
            }
        }
    }
}
