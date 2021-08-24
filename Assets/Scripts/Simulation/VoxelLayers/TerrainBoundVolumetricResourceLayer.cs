using Dman.LSystem.SystemRuntime.VolumetricData;
using Dman.LSystem.SystemRuntime.VolumetricData.Layers;
using Dman.LSystem.SystemRuntime.VolumetricData.NativeVoxels;
using Environment;
using Unity.Burst;
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

        public bool disperse = false;
        [Range(0, 1)]
        public float underTerrainDispersal = 0f;
        [Range(0, 1)]
        public float intersectTerrainDispersal = 0f;
        [Range(0, 1)]
        public float aboveTerrainDispersal = 0f;


        private NativeArray<float> diffusionWeightsByVoxelIndex;
        private NativeArray<float> dispersalFactorByVoxelIndex;
        private JobHandle? diffusionWeightComputeJob = null;
        public override void SetupInternalData(VolumetricWorldVoxelLayout layout)
        {
            diffusionWeightsByVoxelIndex = new NativeArray<float>(layout.totalVoxels, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            dispersalFactorByVoxelIndex = new NativeArray<float>(layout.totalVoxels, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            var sampler = terrainSampler.AsNativeCompatible();

            var weightAssignJob = new DiffusionWeightAssignJob
            {
                diffusionWeightOutputData = diffusionWeightsByVoxelIndex,
                dispersalFactorOutputData = dispersalFactorByVoxelIndex,
                sampler = sampler,
                layout = layout,
                underTerrainWeight = underTerrainDiffusion,
                intersectTerrainWeight = intersectTerrainDiffusion,
                overTerrainWeight = aboveTerrainDiffusion,

                underTerrainDispersal = underTerrainDispersal,
                intersectTerrainDispersal = intersectTerrainDispersal,
                overTerrainDispersal = aboveTerrainDispersal
            };
            var dep = weightAssignJob.Schedule(layout.totalTiles, 100);
            sampler.Dispose(dep);

            diffusionWeightComputeJob = dep;
        }

        public override void CleanupInternalData(VolumetricWorldVoxelLayout layout)
        {
            diffusionWeightsByVoxelIndex.Dispose();
            dispersalFactorByVoxelIndex.Dispose();
        }

        [BurstCompile]
        struct DiffusionWeightAssignJob : IJobParallelFor
        {
            [NativeDisableParallelForRestriction]
            public NativeArray<float> diffusionWeightOutputData;
            [NativeDisableParallelForRestriction]
            public NativeArray<float> dispersalFactorOutputData;
            [ReadOnly]
            public PerlinSamplerNativeCompatable sampler;
            public VolumetricWorldVoxelLayout layout;

            public float underTerrainWeight;
            public float intersectTerrainWeight;
            public float overTerrainWeight;

            public float underTerrainDispersal;
            public float intersectTerrainDispersal;
            public float overTerrainDispersal;

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
                        dispersalFactorOutputData[voxelIndex.Value] = underTerrainDispersal;
                    }
                    else if (bottomOfVoxel > sampleHeight)
                    {
                        // whole voxel is above terrain
                        diffusionWeightOutputData[voxelIndex.Value] = overTerrainWeight;
                        dispersalFactorOutputData[voxelIndex.Value] = overTerrainDispersal;
                    }
                    else
                    {
                        // voxel intersects with terrain
                        diffusionWeightOutputData[voxelIndex.Value] = intersectTerrainWeight;
                        dispersalFactorOutputData[voxelIndex.Value] = intersectTerrainDispersal;
                    }
                }
            }
        }
        public override bool ApplyLayerWideUpdate(VoxelWorldVolumetricLayerData data, float deltaTime, ref JobHandle dependecy)
        {
            var changed = false;
            if(diffuse || disperse)
            {
                if (diffusionWeightComputeJob.HasValue)
                {
                    diffusionWeightComputeJob.Value.Complete();
                    diffusionWeightComputeJob = null;
                }

                var voxelLayout = data.VoxelLayout;
                var workingData = new NativeArray<float>(voxelLayout.totalVoxels, Allocator.TempJob);

                var copyDiffuseInJob = new CopyVoxelToWorkingDataJob
                {
                    layerData = data,
                    targetData = workingData,
                    layerId = voxelLayerId
                };

                dependecy = copyDiffuseInJob.Schedule(workingData.Length, 1000, dependecy);

                if (diffuse)
                {
                    workingData = MyDiffuse(workingData, voxelLayout, deltaTime, ref dependecy);
                    changed = true;
                }
                if (disperse)
                {
                    Disperse(workingData, deltaTime, ref dependecy);
                    changed = true;
                }

                var copyBackJob = new CopyWorkingDataToVoxels
                {
                    layerData = data,
                    sourceData = workingData,
                    layerId = voxelLayerId
                };
                dependecy = copyBackJob.Schedule(workingData.Length, 1000, dependecy);
                workingData.Dispose(dependecy);
            }
            return changed;
        }


        protected NativeArray<float> MyDiffuse(NativeArray<float> workingData, VolumetricWorldVoxelLayout layout, float deltaTime, ref JobHandle dependecy)
        {
            return VoxelAdjacencyByVoxelConstantDiffuser.ComputeDiffusion(
                layout,
                workingData,
                diffusionWeightsByVoxelIndex,
                0.01f,
                deltaTime,
                globalDiffusionConstant,
                ref dependecy);
        }

        protected void Disperse(NativeArray<float> workingData, float deltaTime, ref JobHandle dependency)
        {
            var dispersal = new DisperseByFactor
            {
                reductionByVoxelIndex = dispersalFactorByVoxelIndex,
                resourceData = workingData,
                expReduction = math.min(math.exp(deltaTime) / math.exp(1), 1f) // safeguard against bad behavior if framerate gets too low
            };
            dependency = dispersal.Schedule(workingData.Length, 10000, dependency);
        }

        [BurstCompile]
        struct DisperseByFactor : IJobParallelFor
        {
            [ReadOnly]
            public NativeArray<float> reductionByVoxelIndex;
            public NativeArray<float> resourceData;
            public float expReduction;
            public void Execute(int index)
            {
                resourceData[index] = resourceData[index] * (1 - reductionByVoxelIndex[index] * expReduction);
            }
        }

    }
}