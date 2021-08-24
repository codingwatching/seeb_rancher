using Dman.LSystem.SystemRuntime.VolumetricData;
using Dman.LSystem.SystemRuntime.VolumetricData.NativeVoxels;
using NUnit.Framework;
using Simulation.DOTS.Pathing;
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
            worldSize = Vector3.one,
            dataLayerCount = 1
        };
        var nativeNodeCosts = new VoxelWorldVolumetricLayerData(voxelLayout, Allocator.Persistent);
        for (int x = 0; x < voxelLayout.worldResolution.x; x++)
        {
            for (int y = 0; y < voxelLayout.worldResolution.y; y++)
            {
                for (int z = 0; z < voxelLayout.worldResolution.z; z++)
                {
                    var index = voxelLayout.GetVoxelIndexFromCoordinates(new Vector3Int(x, y, z));
                    nativeNodeCosts[index, 0] = weights[x, y, z];
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


        var frontNodes = new NativeQueue<VoxelIndex>(Allocator.TempJob);
        var infiniteLoopCheck = new NativeArray<bool>(1, Allocator.TempJob);
        JobHandle dep = default;
        try
        {
            dep = initializer.Schedule(parentNodePointers.Length, 100);
            var pather = new DijkstrasPathingSolverJob
            {
                layerData = nativeNodeCosts,
                costAdjustmentLayer = 0,
                parentNodePointers = parentNodePointers,
                tmpPathingData = tmpPathingData,
                frontNodes = frontNodes,

                voxelLayout = voxelLayout,
                targetVoxel = targetVector,
                baseTravelWeight = baseCost,

                failedWithInfiniteLoop = infiniteLoopCheck
            };

            dep = pather.Schedule(dep);

            var expectedParentIndexes = new VoxelIndex[voxelLayout.totalVoxels];
            for (int x = 0; x < voxelLayout.worldResolution.x; x++)
            {
                for (int y = 0; y < voxelLayout.worldResolution.y; y++)
                {
                    for (int z = 0; z < voxelLayout.worldResolution.z; z++)
                    {
                        var baseCoordinate = new Vector3Int(x, y, z);
                        var baseIndex = voxelLayout.GetVoxelIndexFromCoordinates(baseCoordinate);
                        var indicatingChar = expectedParents[x, y][z];
                        var offset = IndicatorCharToOffset(indicatingChar);
                        if (!offset.HasValue)
                        {
                            expectedParentIndexes[baseIndex.Value] = VoxelIndex.Invalid;
                            continue;
                        }

                        var neighborCoord = baseCoordinate + offset.Value;
                        var neighborIndex = voxelLayout.GetVoxelIndexFromCoordinates(neighborCoord);

                        expectedParentIndexes[baseIndex.Value] = neighborIndex;
                    }
                }
            }
            dep.Complete();

            if (infiniteLoopCheck[0])
            {
                Assert.Fail("Pathing resuilted in an infinite loop");
            }

            for (int i = 0; i < voxelLayout.totalVoxels; i++)
            {
                var expectedNeighbor = expectedParentIndexes[i];
                var actualNeighborIndex = parentNodePointers[i];

                if (expectedNeighbor.Value != actualNeighborIndex)
                {
                    Assert.Fail($"Expected parent at {voxelLayout.GetCoordinatesFromVoxelIndex(new VoxelIndex(i))}" +
                        $" to be {voxelLayout.GetCoordinatesFromVoxelIndex(expectedNeighbor)}," +
                        $" but was {voxelLayout.GetCoordinatesFromVoxelIndex(new VoxelIndex(actualNeighborIndex))}");
                }
            }
        }
        finally
        {
            dep.Complete();
            nativeNodeCosts.Dispose();
            parentNodePointers.Dispose();
            tmpPathingData.Dispose();
            frontNodes.Dispose();
            infiniteLoopCheck.Dispose();
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
