using Dman.LSystem.SystemRuntime.VolumetricData;
using Dman.LSystem.SystemRuntime.VolumetricData.Layers;
using Dman.LSystem.SystemRuntime.VolumetricData.NativeVoxels;
using Environment;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Simulation.VoxelLayers
{
    [CreateAssetMenu(fileName = "TerrainBoundVolumetricResourceLayer", menuName = "LSystem/TerrainBoundVolumetricResourceLayer")]
    public class TerrainBoundVolumetricResourceLayer : VolumetricResourceLayer
    {
        public PerlinSampler terrainSampler;
        public float underTerrainDiffusion = 1f;
        public float intersectTerrainDiffusion = 1f;
        public float aboveTerrainDiffusion = 0.1f;

        private NativeArray<float> diffusionWeightsByVoxelIndex;
        private JobHandle? diffusionWeightComputeJob = null;
        public override void SetupInternalData(VolumetricWorldVoxelLayout layout)
        {
            diffusionWeightsByVoxelIndex = new NativeArray<float>(layout.totalVoxels, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            var sampler = terrainSampler.AsNativeCompatible();

            var weightAssignJob = new DiffusionWeightAssignJob
            {
                diffusionWeightOutputData = diffusionWeightsByVoxelIndex,
                sampler = sampler,
                layout = layout,
                underTerrainWeight = underTerrainDiffusion,
                intersectTerrainWeight = intersectTerrainDiffusion,
                overTerrainWeight = aboveTerrainDiffusion
            };
            var dep = weightAssignJob.Schedule(layout.totalTiles, 100);
            sampler.Dispose(dep);

            diffusionWeightComputeJob = dep;
        }

        struct DiffusionWeightAssignJob : IJobParallelFor
        {
            [NativeDisableParallelForRestriction]
            public NativeArray<float> diffusionWeightOutputData;
            [ReadOnly]
            public PerlinSamplerNativeCompatable sampler;
            public VolumetricWorldVoxelLayout layout;

            public float underTerrainWeight;
            public float intersectTerrainWeight;
            public float overTerrainWeight;

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
                        diffusionWeightOutputData[voxelIndex.Value] = underTerrainWeight;
                    }
                    else if (bottomOfVoxel > sampleHeight)
                    {
                        // whole voxel is above terrain
                        diffusionWeightOutputData[voxelIndex.Value] = overTerrainWeight;
                    }
                    else
                    {
                        // voxel intersects with terrain
                        diffusionWeightOutputData[voxelIndex.Value] = intersectTerrainWeight;
                    }
                }
            }
        }

        protected override JobHandle Diffuse(VoxelWorldVolumetricLayerData data, float deltaTime, JobHandle dependecy)
        {
            if (diffusionWeightComputeJob.HasValue)
            {
                diffusionWeightComputeJob.Value.Complete();
                diffusionWeightComputeJob = null;
            }
            var voxelLayout = data.VoxelLayout;
            var diffusionData = new NativeArray<float>(voxelLayout.totalVoxels, Allocator.TempJob);

            var copyDiffuseInJob = new CopyVoxelToWorkingDataJob
            {
                layerData = data,
                targetData = diffusionData,
                layerId = voxelLayerId
            };

            dependecy = copyDiffuseInJob.Schedule(diffusionData.Length, 1000, dependecy);

            var resultArray = VoxelAdjacencyByVoxelConstantDiffuser.ComputeDiffusion(
                voxelLayout,
                diffusionData,
                diffusionWeightsByVoxelIndex,
                0.01f,
                deltaTime,
                globalDiffusionConstant,
                ref dependecy);

            var copyBackJob = new CopyWorkingDataToVoxels
            {
                layerData = data,
                sourceData = resultArray,
                layerId = voxelLayerId
            };
            dependecy = copyBackJob.Schedule(diffusionData.Length, 1000, dependecy);

            resultArray.Dispose(dependecy);

            return dependecy;
        }

    }
}