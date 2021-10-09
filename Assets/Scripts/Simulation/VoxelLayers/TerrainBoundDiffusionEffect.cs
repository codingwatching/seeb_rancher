using Dman.LSystem;
using Dman.LSystem.SystemRuntime.NativeCollections.NativeVolumetricSpace;
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
    [CreateAssetMenu(fileName = "TerrainBoundDiffusionEffect", menuName = "LSystem/Resource Layers/TerrainBoundDiffusionEffect")]
    public class TerrainBoundDiffusionEffect : VolumetricLayerEffect
    {
        public float globalDiffusionConstant = 1f;

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

            var weightAssignJob = new PerlinSampledVoxelAssignment
            {
                perVoxelIdOutput = diffusionWeightsByVoxelIndex,
                sampler = sampler,
                layout = layout,
                underTerrainValue = underTerrainDiffusion,
                intersectTerrainValue = intersectTerrainDiffusion,
                overTerrainValue = aboveTerrainDiffusion,
            };
            var dep = weightAssignJob.Schedule(layout.totalTiles, 100);
            sampler.Dispose(dep);

            diffusionWeightComputeJob = dep;
        }

        public override void CleanupInternalData(VolumetricWorldVoxelLayout layout)
        {
            diffusionWeightsByVoxelIndex.Dispose();
        }

        public override bool ApplyEffectToLayer(DoubleBuffered<float> layerData, VoxelWorldVolumetricLayerData readonlyLayerData, float deltaTime, ref JobHandleWrapper dependecy)
        {
            if (diffusionWeightComputeJob.HasValue)
            {
                diffusionWeightComputeJob.Value.Complete();
                diffusionWeightComputeJob = null;
            }

            VoxelAdjacencyByVoxelConstantDiffuser.ComputeDiffusion(
                readonlyLayerData.VoxelLayout,
                layerData,
                diffusionWeightsByVoxelIndex,
                0.01f,
                deltaTime,
                globalDiffusionConstant,
                ref dependecy);

            return true;
        }
    }
}