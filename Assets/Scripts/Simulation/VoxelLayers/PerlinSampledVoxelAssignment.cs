using Dman.LSystem.SystemRuntime.VolumetricData;
using Dman.LSystem.SystemRuntime.VolumetricData.NativeVoxels;
using Environment;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Simulation.VoxelLayers
{

    [BurstCompile]
    public struct PerlinSampledVoxelAssignment : IJobParallelFor
    {
        [NativeDisableParallelForRestriction]
        public NativeArray<float> perVoxelIdOutput;
        [ReadOnly]
        public PerlinSamplerNativeCompatable sampler;
        public VolumetricWorldVoxelLayout layout;

        public float underTerrainValue;
        public float intersectTerrainValue;
        public float overTerrainValue;

        public void Execute(int index)
        {
            var tileIndex = new TileIndex(index);
            var tileCoordinate = layout.SurfaceGetCoordinatesFromTileIndex(tileIndex);
            var tilePosition = layout.SurfaceToCenterOfTile(tileCoordinate);
            var sampleHeight = sampler.SampleNoise(tilePosition);

            for (int y = 0; y < layout.worldResolution.y; y++)
            {
                var voxelCoord = new Vector3Int(tileCoordinate.x, y, tileCoordinate.y);
                var voxelIndex = layout.GetVoxelIndexFromCoordinates(voxelCoord);
                var voxelPos = layout.CoordinateToCenterOfVoxel(voxelCoord);

                var bottomOfVoxel = math.floor(voxelPos.y);
                var topOfVoxel = math.ceil(voxelPos.y);
                if (topOfVoxel < sampleHeight)
                {
                    // whole voxel is below terrain
                    perVoxelIdOutput[voxelIndex.Value] = underTerrainValue;
                }
                else if (bottomOfVoxel > sampleHeight)
                {
                    // whole voxel is above terrain
                    perVoxelIdOutput[voxelIndex.Value] = overTerrainValue;
                }
                else
                {
                    // voxel intersects with terrain
                    perVoxelIdOutput[voxelIndex.Value] = intersectTerrainValue;
                }
            }
        }
    }
}