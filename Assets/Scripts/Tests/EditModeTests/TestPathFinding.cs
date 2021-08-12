using Assets.Scripts.PlantPathing;
using Dman.LSystem.SystemRuntime.VolumetricData;
using NUnit.Framework;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class TestPathFinding
{
    /// <summary>
    /// expected neighbors consists of 6 characters, one for each possible neigbor:
    /// 
    /// < : -z
    /// > : +z
    /// ^ : -y
    /// v : +y
    /// . : -x
    /// o : +x
    /// </summary>
    /// <param name="weights"></param>
    /// <param name="baseCost"></param>
    /// <param name="targetVector"></param>
    /// <param name="expectedParents"></param>
    private void TestPathfindResults(float baseCost, Vector3Int targetVector, float[,,] weights, string[,] expectedParents)
    {
        var voxelLayout = new VolumetricWorldVoxelLayout
        {
            voxelOrigin = Vector3.zero,
            worldResolution = new Vector3Int(
                weights.GetLength(0),
                weights.GetLength(1),
                weights.GetLength(2)),
            worldSize = Vector3.one
        };
        var nativeNodeCosts = new NativeArray<float>(voxelLayout.totalVolumeDataSize, Allocator.Persistent);
        for (int x = 0; x < voxelLayout.worldResolution.x; x++)
        {
            for (int y = 0; y < voxelLayout.worldResolution.y; y++)
            {
                for (int z = 0; z < voxelLayout.worldResolution.z; z++)
                {
                    var index = voxelLayout.GetDataIndexFromCoordinates(new Vector3Int(x, y, z));
                    nativeNodeCosts[index] = weights[x, y, z];
                }
            }
        }

        var parentNodePointers = new NativeArray<int>(nativeNodeCosts.Length, Allocator.Persistent);
        var tmpPathingData = new NativeArray<DijkstrasPathingSolverJob.PathingNodeTmpData>(nativeNodeCosts.Length, Allocator.Persistent);

        var initializer = new DijkstrasPathingSolverInitializerJob
        {
            parentNodePointers = parentNodePointers,
            tmpPathingData = tmpPathingData
        };


        var frontNodes = new NativeQueue<int>(Allocator.TempJob);
        try
        {
            var dep = initializer.Schedule(parentNodePointers.Length, 100);
            var pather = new DijkstrasPathingSolverJob
            {
                nodeCostAdjustmentVoxels = nativeNodeCosts,
                parentNodePointers = parentNodePointers,
                tmpPathingData = tmpPathingData,
                frontNodes = frontNodes,

                voxelLayout = voxelLayout,
                targetVoxel = targetVector,
                baseTravelWeight = baseCost
            };

            dep = pather.Schedule(dep);

            var expectedParentIndexes = new int[voxelLayout.totalVolumeDataSize];
            for (int x = 0; x < voxelLayout.worldResolution.x; x++)
            {
                for (int y = 0; y < voxelLayout.worldResolution.y; y++)
                {
                    for (int z = 0; z < voxelLayout.worldResolution.z; z++)
                    {
                        var baseCoordinate = new Vector3Int(x, y, z);
                        var baseIndex = voxelLayout.GetDataIndexFromCoordinates(baseCoordinate);
                        var indicatingChar = expectedParents[x, y][z];
                        var offset = IndicatorCharToOffset(indicatingChar);
                        if (!offset.HasValue)
                        {
                            expectedParentIndexes[baseIndex] = -1;
                            continue;
                        }

                        var neighborCoord = baseCoordinate + offset.Value;
                        var neighborIndex = voxelLayout.GetDataIndexFromCoordinates(neighborCoord);

                        expectedParentIndexes[baseIndex] = neighborIndex;
                    }
                }
            }
            dep.Complete();

            for (int i = 0; i < voxelLayout.totalVolumeDataSize; i++)
            {
                var expectedNeighbor = expectedParentIndexes[i];
                var actualNeighborIndex = parentNodePointers[i];

                if (expectedNeighbor != actualNeighborIndex)
                {
                    Assert.Fail($"Expected parent at {voxelLayout.GetCoordinatesFromDataIndex(i)} to be {voxelLayout.GetCoordinatesFromDataIndex(expectedNeighbor)}, but was {voxelLayout.GetCoordinatesFromDataIndex(actualNeighborIndex)}");
                }
            }
        }
        finally
        {
            nativeNodeCosts.Dispose();
            parentNodePointers.Dispose();
            tmpPathingData.Dispose();
            frontNodes.Dispose();
        }
    }

    private Vector3Int? IndicatorCharToOffset(char indicator)
    {
        switch (indicator)
        {
            case '>':
                return new Vector3Int(0, 0, 1);
            case '<':
                return new Vector3Int(0, 0, -1);
            case '^':
                return new Vector3Int(0, -1, 0);
            case 'v':
                return new Vector3Int(0, 1, 0);
            case '.':
                return new Vector3Int(-1, 0, 0);
            case 'o':
                return new Vector3Int(1, 0, 0);
            case '-':
                return null;
            default:
                throw new System.Exception("unrecognized indicator character " + indicator);
                break;
        }
    }

    [Test]
    public void TestPathFindingNoObstacles()
    {
        TestPathfindResults(
            1,
            new Vector3Int(0, 1, 1),
            new float[,,] {
                {
                    {0, 0, 0, 0 },
                    {0, 0, 0, 0 },
                    {0, 0, 0, 0 },
                    {0, 0, 0, 0 },
                } },
            new[,] {
                {
                    ">v<<" ,
                    ">-<<",
                    ">^<<",
                    ">^<<",
                } }
            );
    }
    [Test]
    public void TestPathFindingWithCostlyWall()
    {
        TestPathfindResults(
            1,
            new Vector3Int(0, 1, 1),
            new float[,,] {
                {
                    {0, 0, 0, 0 },
                    {0, 0, 0, 0 },
                    {9, 9, 9, 0 },
                    {0, 0, 0, 0 },
                } },
            new[,] {
                {
                    ">v<<" ,
                    ">-<<",
                    "^^^^",
                    ">>>^",
                } }
            );
    }
    [Test]
    public void TestPathFindingWithChoices()
    {
        TestPathfindResults(
            1,
            new Vector3Int(0, 0, 2),
            new float[,,] {
                {
                    {0, 0, 0, 0, 0 },
                    {0, 0, 0, 0, 0 },
                    {4, 5, 9, 8, 7 },
                    {0, 0, 0, 0, 0 },
                    {0, 0, 0, 0, 0 },
                } },
            new[,] {
                {
                    ">>-<<",
                    ">>^<<",
                    "^^^^^",
                    "^^<<^",
                    "^^<<^",
                } }
            );
    }
}
