using Dman.LSystem.SystemRuntime.VolumetricData;
using System.Collections;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Assets.Scripts.PlantPathing
{

    public struct DijkstrasPathingSolverInitializerJob : IJobParallelFor
    {
        public NativeArray<int> parentNodePointers;
        public NativeArray<DijkstrasPathingSolverJob.PathingNodeTmpData> tmpPathingData;
        public void Execute(int index)
        {
            parentNodePointers[index] = -1;
            tmpPathingData[index] = DijkstrasPathingSolverJob.PathingNodeTmpData.DefaultData();
        }
    }

    /// <summary>
    /// solves and caches a pathing grid, which paths to a specific target voxel using Dijkstra's algorithm
    /// </summary>
    public struct DijkstrasPathingSolverJob : IJob
    {
        public VolumetricWorldVoxelLayout voxelLayout;
        public NativeArray<float> nodeCostAdjustmentVoxels;

        // working data
        public NativeArray<int> parentNodePointers;
        public NativeArray<PathingNodeTmpData> tmpPathingData;

        public NativeQueue<int> frontNodes;

        public Vector3Int targetVoxel;
        public float baseTravelWeight;

        public void Execute()
        {
            var firstIndex = voxelLayout.GetDataIndexFromCoordinates(targetVoxel);
            var firstData = tmpPathingData[firstIndex];
            firstData.totalTravelCost = 0;
            tmpPathingData[firstIndex] = firstData;

            frontNodes.Enqueue(firstIndex);

            while (frontNodes.Count > 0)
            {
                var nextNode = frontNodes.Dequeue();
                Visit(nextNode);
            }
        }

        private void Visit(int index)
        {
            var data = tmpPathingData[index];
            var coordinate = voxelLayout.GetCoordinatesFromDataIndex(index);

            CheckNeighbor(data, index, coordinate + new Vector3Int( 1,  0,  0));
            CheckNeighbor(data, index, coordinate + new Vector3Int(-1,  0,  0));
            CheckNeighbor(data, index, coordinate + new Vector3Int( 0,  1,  0));
            CheckNeighbor(data, index, coordinate + new Vector3Int( 0, -1,  0));
            CheckNeighbor(data, index, coordinate + new Vector3Int( 0,  0,  1));
            CheckNeighbor(data, index, coordinate + new Vector3Int( 0,  0,  -1));
        }

        private void CheckNeighbor(PathingNodeTmpData sourceNode, int sourceIndex, Vector3Int targetCoordinate)
        {
            var neighborIndex = voxelLayout.GetDataIndexFromCoordinates(targetCoordinate);
            if(neighborIndex < 0)
            {
                return;
            }
            var neighborNode = tmpPathingData[neighborIndex];
            var travelCost = nodeCostAdjustmentVoxels[neighborIndex] + baseTravelWeight;
            var newWeight = travelCost + sourceNode.totalTravelCost;
            if(newWeight < neighborNode.totalTravelCost)
            {
                neighborNode.totalTravelCost = newWeight;
                tmpPathingData[neighborIndex] = neighborNode;
                parentNodePointers[neighborIndex] = sourceIndex;
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
